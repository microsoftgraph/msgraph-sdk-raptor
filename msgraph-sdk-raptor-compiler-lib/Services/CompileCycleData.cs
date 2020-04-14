using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MsGraphSDKSnippetsCompiler.Data;
using MsGraphSDKSnippetsCompiler.Models;

namespace MsGraphSDKSnippetsCompiler.Services
{
    public class CompileCycleData : ICompileCycle
    {
        private readonly RaptorDbContext _context;

        public CompileCycleData(RaptorDbContext context)
        {
            _context = context;
        }

        public CompileCycle Add(CompileCycle compileCycle)
        {
            _context.CompileCycle.Add(compileCycle);
            _context.SaveChanges();
            return compileCycle;
        }

        public CompileCycle Get(Guid id)
        {
            return _context.CompileCycle.FirstOrDefault(r => r.CompileCycleID == id);
        }

        public IEnumerable<CompileCycle> GetAll()
        {
            return _context.CompileCycle.OrderBy(r => r.CompileDate);
        }

        public CompileCycle Update(CompileCycle compileCycle)
        {
            _context.Attach(compileCycle).State =
               EntityState.Modified;
               _context.SaveChanges();
            return compileCycle;
        }
    }
}