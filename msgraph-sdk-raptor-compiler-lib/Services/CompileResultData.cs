using Microsoft.EntityFrameworkCore;
using MsGraphSDKSnippetsCompiler.Data;
using MsGraphSDKSnippetsCompiler.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MsGraphSDKSnippetsCompiler.Services
{
    public class CompileResultData : ICompileResult
    {
        private readonly RaptorDbContext _context;

        public CompileResultData(RaptorDbContext context)
        {
            _context = context;
        }

        public CompileResult Add(CompileResult compileResult)
        {
            _context.CompileResult.Add(compileResult);
            _context.SaveChanges();
            return compileResult;
        }

        public CompileResult Get(Guid id)
        {
            return _context.CompileResult.FirstOrDefault(r => r.CompileResultsID == id);
        }

        public IEnumerable<CompileResult> GetAll()
        {
            return _context.CompileResult.OrderBy(r => r.CompileResultsID);
        }

        public CompileResult Update(CompileResult compileResult)
        {
            _context.Attach(compileResult).State =
              EntityState.Modified;
              _context.SaveChanges();
            return compileResult;
        }
    }
}