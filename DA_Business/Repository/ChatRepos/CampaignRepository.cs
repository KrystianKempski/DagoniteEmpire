﻿using AutoMapper;
using DA_Business.Repository.CharacterReps.IRepository;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Chat;
using DA_DataAccess.Data;
using DA_Models.CharacterModels;
using DagoniteEmpire.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;

namespace DA_Business.Repository.ChatRepos
{
    public class CampaignRepository : ICampaignRepository
    {
        //private readonly ApplicationDbContext _db;
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IMapper _mapper;

        public CampaignRepository(IDbContextFactory<ApplicationDbContext> db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<CampaignDTO> Create(CampaignDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = _mapper.Map<CampaignDTO, Campaign>(objDTO);

                foreach(var cha in obj.Characters)
                {
                    cha.Profession = null;
                    cha.Race = null;
                }

                var characterts = await contex.Characters.ToListAsync();
                characterts.ForEach(t =>
                {
                    if (obj.Characters.Any(nt => nt.Id == t.Id))
                    {
                        var untracked = obj.Characters.FirstOrDefault(nt => nt.Id == t.Id);
                        obj.Characters.Remove(untracked);
                        obj.Characters.Add(t);
                    }
                });

                var addedObj = await contex.Campaigns.AddAsync(obj);
                await contex.SaveChangesAsync();
                return _mapper.Map<Campaign, CampaignDTO>(addedObj.Entity);
            }
            catch (Exception ex) { 
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + ex.Message); 
            }
            
        }

        public async Task<bool> CheckIfCampaignBelongToUser(string userName, int campaignId)
        {
            using var contex = await _db.CreateDbContextAsync();
            var obj = await contex.Campaigns.Include(c=>c.Characters).FirstOrDefaultAsync(i=>i.Id == campaignId);

            if (obj is null) return false;

            var charac = obj.Characters.FirstOrDefault(c => c.UserName == userName);

            return charac is not null;
        }

        public async Task<int> Delete(int id)
        {
            try
            {

                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.Campaigns.FirstOrDefaultAsync(u => u.Id == id);
                if (obj.Chapters != null && obj.Chapters.Any())
                {
                    foreach (var chap in obj.Chapters)
                    {
                        if (chap.Posts != null && chap.Posts.Any())
                        {
                            foreach (var mess in chap.Posts)
                            {
                                contex.Posts.Remove(mess);
                            }
                        }
                        contex.Chapters.Remove(chap);
                    }
                }
                if (obj != null)
                {
                    contex.Campaigns.Remove(obj);
                    return await contex.SaveChangesAsync();
                }
            }
            catch (Exception ex) { 
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + ex.Message);
            }
            return 0;
        }

        public async Task<IEnumerable<CampaignDTO>> GetAll(int? characterId = null)
        {
            try
            {

                using var contex = await _db.CreateDbContextAsync();
                if (characterId==null)
                    return _mapper.Map<IEnumerable<Campaign>, IEnumerable<CampaignDTO>>(contex.Campaigns.Include(a => a.Characters));

                var obj = contex.Campaigns.Include(a=>a.Characters).Where(u => u.Characters.FirstOrDefault(a=>a.Id==characterId)!=null).OrderBy(u => u.CreatedDate);
                if (obj != null && obj.Any())
                    return _mapper.Map<IEnumerable<Campaign>, IEnumerable<CampaignDTO>>(obj);
                else
                    return new List<CampaignDTO>();
               

            }
            catch (Exception ex) { 
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + ex.Message); 
            }
        }

        public async Task<CampaignDTO> GetById(int id)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.Campaigns.Include(a => a.Characters).FirstOrDefaultAsync(u => u.Id == id);
                if (obj != null)
                {
                    return _mapper.Map<Campaign, CampaignDTO>(obj);
                }
            }
            catch (Exception ex) {
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + ex.Message); 
            }
            return new CampaignDTO();
        }

        public async Task<CampaignDTO> Update(CampaignDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.Campaigns.Include(a => a.Characters).FirstOrDefaultAsync(u => u.Id == objDTO.Id);
                if (obj is not null)
                {
                    var updatedCampaign = _mapper.Map<CampaignDTO, Campaign>(objDTO);

                    // Update parent
                    contex.Entry(obj).CurrentValues.SetValues(updatedCampaign);


                    // Delete characters
                    if (obj.Characters is not null)
                    {
                        foreach (var existingChild in obj.Characters.ToList())
                        {
                            if (!updatedCampaign.Characters.Any(c => c.Id == existingChild.Id))
                            {
                                //remove from campaign
                                var detachedCharacter = contex.Characters.Include(c => c.Campaigns).Include(c => c.Chapters).FirstOrDefault(c => c.Id == existingChild.Id && c.Id != default(int));
                                if (detachedCharacter == null || detachedCharacter.Campaigns == null || !detachedCharacter.Campaigns.Contains(obj))
                                    continue;
                                detachedCharacter.Campaigns.Remove(obj);
                                contex.Characters.Update(detachedCharacter);
                                // also remove from chapters
                                var allChapters = contex.Chapters.Include(c => c.Characters).Where(c => c.CampaignId == obj.Id);
                                if(allChapters.Any())
                                {
                                    foreach(var chapter in allChapters)
                                    {
                                        detachedCharacter.Chapters.Remove(chapter);
                                        contex.Characters.Update(detachedCharacter);
                                    }
                                }
                            }
                        }
                    }

                    // Update and Insert Characters
                    if (updatedCampaign.Characters is not null)
                    {
                        foreach (var childChar in updatedCampaign.Characters)
                        {
                            if (!obj.Characters.Any(c => c.Id == childChar.Id && c.Id != default(int)))
                            {
                                var existingCharacter = contex.Characters.Include(u=>u.Campaigns).FirstOrDefault(c => c.Id == childChar.Id && c.Id != default(int));
                                if(existingCharacter is not null && existingCharacter.Campaigns is not null)
                                {
                                    //obj.Characters.Add(childChar);
                                    existingCharacter.Campaigns.Add(obj);
                                    contex.Characters.Update(existingCharacter);
                                }
                            }
                        }
                    }

                    await contex.SaveChangesAsync();
                    return _mapper.Map<Campaign, CampaignDTO>(obj);
                }
                else
                {
                    obj = _mapper.Map<CampaignDTO, Campaign>(objDTO);


                    foreach (var cha in obj.Characters)
                    {
                        cha.Profession = null;
                        cha.Race = null;
                    }

                    var characterts = await contex.Characters.ToListAsync();
                    characterts.ForEach(t =>
                    {
                        if (obj.Characters.Any(nt => nt.Id == t.Id))
                        {
                            var untracked = obj.Characters.FirstOrDefault(nt => nt.Id == t.Id);
                            obj.Characters.Remove(untracked);
                            obj.Characters.Add(t);
                        }
                    });

                    var addedObj = contex.Campaigns.Add(obj);
                    await contex.SaveChangesAsync();

                    return _mapper.Map<Campaign, CampaignDTO>(addedObj.Entity);
                }
            }
            catch (Exception ex) {
                throw new RepositoryErrorException("Error in" + System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + ex.Message);
            }
        }
    }
}
