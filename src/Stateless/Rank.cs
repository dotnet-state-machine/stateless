using System.Collections.Generic;

namespace Stateless.Dot
{
    class Rank
    {
        public List<string> EntriesList ;
        public List<string> ExitsList;
        public List<string> ClustersList;

        public Rank()
        {
            ClustersList = new List<string>();
        }
        public static List<string> TryAdd(List<string> list, string entity)
        {
            try
            {
                list.Add(entity);
            }
            catch
            {
                // ignored
            }
            return list;
        }

        public override string ToString()
        {
            return string.Empty;  //$"\n{{ rank=min;{string.Join(";", EntriesList)} }}\n" +
            //$"{{ rank=same;{string.Join(";", ClustersList)} }}\n" +
            //$"{{ rank=max;{string.Join(";", ExitsList)} }}\n";
        }
    }
}