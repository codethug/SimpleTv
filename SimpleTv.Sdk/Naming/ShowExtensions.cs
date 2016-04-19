using SimpleTv.Sdk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.VisualBasic;

namespace SimpleTv.Sdk.Naming
{
    public static class ShowExtensions
    {
        public static IEnumerable<T> IncludeOnly<T>(this IEnumerable<T> shows, string includeFilter) where T : INamed
        {
            return shows.Where(s =>
                Operators.LikeString(s.Name, includeFilter, CompareMethod.Text)
            );
        }

        public static IEnumerable<T> Exclude<T>(this IEnumerable<T> shows, string excludeFilter) where T : INamed
        {
            return shows.Where(s =>
                !Operators.LikeString(s.Name, excludeFilter, CompareMethod.Text)
            );
        }
    }
}
