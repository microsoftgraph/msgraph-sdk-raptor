using MsGraphSDKSnippetsCompiler.Data;
using MsGraphSDKSnippetsCompiler.Models;
using System.Collections.Generic;
using System.Linq;

namespace MsGraphSDKSnippetsCompiler.Services
{
    public class CompileResultsViewData
    {
        private readonly RaptorDbContext _context;

        public CompileResultsViewData(RaptorDbContext context)
        {
            _context = context;
        }

        public IEnumerable<CompileResultsView> GetAll()
        {
            return _context.CompileResultsView.OrderBy(r => r.CompileDate);
        }
    }
}