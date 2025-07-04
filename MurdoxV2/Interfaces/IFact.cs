using MurdoxV2.Models;
using MurdoxV2.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Interfaces
{
    public interface IFact
    {
        Task<Result<List<Fact>, SystemError<FactDataServiceProvider>>> GetFactsAsync();
        Task<Result<Fact, SystemError<FactDataServiceProvider>>> GetRandomFactAsync();
    }
}
