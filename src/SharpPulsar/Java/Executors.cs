using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//https://www.twblogs.net/a/5ef5f2ddb30d2a4cfd4c3c91
namespace SharpPulsar.Java
{
    public static class Executors
    {//“翻譯”的java中的構造類，也可以不通過它創建線程池
        public static ScheduledExecutorService NewSingleThreadScheduledExecutor()
        {
            return new ScheduledExecutorService();
        }
    }
}
