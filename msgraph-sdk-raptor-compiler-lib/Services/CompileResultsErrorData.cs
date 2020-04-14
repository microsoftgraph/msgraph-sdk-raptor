using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MsGraphSDKSnippetsCompiler.Data;
using MsGraphSDKSnippetsCompiler.Models;

namespace MsGraphSDKSnippetsCompiler.Services
{
    public class CompileResultsErrorData : ICompileResultsError
    {
        private readonly RaptorDbContext _context;

        public CompileResultsErrorData(RaptorDbContext context)
        {
            _context = context;
        }

        public CompileResultsError Add(CompileResultsError compileResultsError)
        {
            _context.CompileResultsError.Add(compileResultsError);
            _context.SaveChanges();
            return compileResultsError;
        }

        public CompileResultsError Get(Guid id)
        {
            return _context.CompileResultsError.FirstOrDefault(r => r.CompileResultsErrorID == id);
        }

        public IEnumerable<CompileResultsError> GetAll()
        {
            return _context.CompileResultsError.OrderBy(r => r.CompileResultsErrorID);
        }

        public CompileResultsError Update(CompileResultsError compileResultsError)
        {
            _context.Attach(compileResultsError).State =
              EntityState.Modified;
            _context.SaveChanges();
            return compileResultsError;
        }
    }
}