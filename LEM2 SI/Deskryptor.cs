using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LEM2_SI
{
    //pozniej mozna setter getter +/- 1
    class Deskryptor
    {
        public int obiekt;
        public int argument;
        public int wartosc;
        public int pokrycie;

        public Deskryptor(int obiekt, int argument, int wartosc, int pokrycie)
        {
            this.obiekt = obiekt;
            this.argument = argument;
            this.wartosc = wartosc;
            this.pokrycie = pokrycie;
        }
    }
}
