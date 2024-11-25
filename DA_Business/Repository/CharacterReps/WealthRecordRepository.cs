using Abp.Collections.Extensions;
using AutoMapper;
using Castle.MicroKernel.Registration;
using DA_Business.Repository.CharacterReps.IRepository;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Data;
using DA_Models.CharacterModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DagoniteEmpire.Exceptions;
using System.Runtime.Remoting;

namespace DA_Business.Repository.CharacterReps
{
    public class WealthRecordRepository : IWealthRecordRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly IMapper _mapper;

        public WealthRecordRepository(IDbContextFactory<ApplicationDbContext> db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<WealthRecordDTO> Create(WealthRecordDTO objDTO)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = _mapper.Map<WealthRecordDTO, WealthRecord>(objDTO);


                var addedObj = await contex.WealthRecords.AddAsync(obj);
                await contex.SaveChangesAsync();

                return _mapper.Map<WealthRecord, WealthRecordDTO>(addedObj.Entity);
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in WealthRecords Repository Create: " + ex.Message);
            }                
        }

        public async Task<int> Delete(int id)
        {
            try
            {
                using var contex = await _db.CreateDbContextAsync();
                var obj = await contex.WealthRecords.FirstOrDefaultAsync(u => u.Id == id);
                if (obj is not null)
                {                    
                    contex.WealthRecords.Remove(obj);
                    await contex.SaveChangesAsync();
                }
            return 0;
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in WealthRecords Repository Delete: " + ex.Message);
            }
        }

        public async Task<IEnumerable<WealthRecordDTO>> GetAll(int? charId = null)
        {
            using var contex = await _db.CreateDbContextAsync();
            if(contex.WealthRecords.Any() == false) 
                return Enumerable.Empty<WealthRecordDTO>();
            if (charId == null || charId < 1)
                return _mapper.Map<IEnumerable<WealthRecord>, IEnumerable<WealthRecordDTO>>(contex.WealthRecords);
            return _mapper.Map<IEnumerable<WealthRecord>, IEnumerable<WealthRecordDTO>>(contex.WealthRecords.Where(u => u.CharacterId == charId));
        }


        public async Task<WealthRecordDTO> GetById(int id)
        {
            using var contex = await _db.CreateDbContextAsync();
            if (contex.WealthRecords.Any() == false)
                return new WealthRecordDTO();
            var obj = await contex.WealthRecords.FirstOrDefaultAsync(u => u.Id == id);
            if (obj != null)
            {
                return _mapper.Map<WealthRecord, WealthRecordDTO>(obj);
            }
            return new WealthRecordDTO();
        }

        public async Task<WealthRecordDTO> Update(WealthRecordDTO objDTO)
        {
            try
            {
                using var context = await _db.CreateDbContextAsync();
                var existingRecord = await context.WealthRecords.FirstOrDefaultAsync(w => w.Id == objDTO.Id);
                if (existingRecord == null)
                {
                    throw new RepositoryErrorException("Wealth record not found");
                }

                _mapper.Map(objDTO, existingRecord);
                context.WealthRecords.Update(existingRecord);
                await context.SaveChangesAsync();

                return _mapper.Map<WealthRecord, WealthRecordDTO>(existingRecord);
            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in WealthRecords Repository Update: " + ex.Message);
            }
        }
        public async Task<WealthRecordDTO> Update(WealthRecordDTO objDTO)
        {
            try
            {

                //await contex.SaveChangesAsync();
                //return _mapper.Map<EquipmentSlot, EquipmentSlotDTO>(obj);
                return null;

            }
            catch (Exception ex)
            {
                throw new RepositoryErrorException("Error in WealthRecords Repository Update: " + ex.Message);
            }
        }
    }
}
