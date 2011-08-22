namespace ZMP.Decorate
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class ActorEnumerableExtensions
    {
        public static IEnumerable<Actor> OfName(this IEnumerable<Actor> actors, string name)
        {
            string lowerCaseName = name.ToLower();

            return
                from actor in actors
                where actor.Name == lowerCaseName
                select actor;
        }
    }
}
