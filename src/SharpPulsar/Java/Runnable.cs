using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpPulsar.Java
{
    public class Runnable
    { //java中沒有委託，這個類相當於一個萬能委託事件的類
        public Runnable(Action handle)
        {
            thisEvent += handle;
        }
        event Action thisEvent;
        public void Run()
        {
            if (thisEvent != null)
                thisEvent();
        }
        public Action GetEvent()
        {
            return thisEvent;
        }
    }
}
