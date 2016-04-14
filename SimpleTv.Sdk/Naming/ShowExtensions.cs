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
        public static IEnumerable<Show> IncludeOnly(this IEnumerable<Show> shows, string includeFilter)
        {
            return shows.Where(s =>
                Operators.LikeString(s.Name, includeFilter, CompareMethod.Text)
            );
        }

        public static IEnumerable<Show> Exclude(this IEnumerable<Show> shows, string excludeFilter)
        {
            return shows.Where(s =>
                !Operators.LikeString(s.Name, excludeFilter, CompareMethod.Text)
            );
        }

    }
}
