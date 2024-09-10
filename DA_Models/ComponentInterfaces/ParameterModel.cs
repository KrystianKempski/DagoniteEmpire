using Abp.Collections.Extensions;
using DA_DataAccess.Chat;
using DA_Models.CharacterModels;
using DA_Models.ComponentModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.ComponentInterfaces
{

    public class ParameterModel<T>
    {
        protected readonly AllParamsModel _allParams;

        public ParameterModel(AllParamsModel allParams)
        {
            _allParams = allParams;
        }

        protected IDictionary<string, T> Properties { get; set; } = new Dictionary<string, T>();
        public virtual IDictionary<string, T> GetAll()
        {
            return Properties;
        }

        public virtual IEnumerable<T> GetAllArray()
        {
            return Properties.Values;
        }

        public virtual T? Get(string key)
        {
            if (key.IsNullOrEmpty())
                return default;
            return Properties[key];
        }
        public virtual T? Set(string key,T value)
        {
            if (key.IsNullOrEmpty())
                return default(T);
            return Properties[key] = value;
        }

        public virtual void Init(IDictionary<string, T> properties)
        {
            Properties = properties;
        }
  
    }
}
