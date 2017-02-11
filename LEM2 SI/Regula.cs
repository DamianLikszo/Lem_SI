using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LEM2_SI
{
    class Regula
    {
        static int liczbaRegul = 0;

        public List<Deskryptor> deskryptory;
        public List<int> obiekty;
        public int pokrycie;
        public int decyzja;
        public int numer;

        public Regula(int koncept)
        {
            liczbaRegul++;

            this.deskryptory = new List<Deskryptor>();
            this.obiekty = new List<int>();
            this.pokrycie = 0;
            this.decyzja = koncept;
            this.numer = liczbaRegul;
        }

        public void Dodaj(Deskryptor deskryptor)
        {
            deskryptory.Add(deskryptor);
        }

        // ktore obiekty pokrywa regula, wpisuje tez pokrycie w regule oraz do jakich obiektow sie odnosi
        public List<int> PokrycieObj(int[,] aSystemDecyzyjny)
        {
            List<int> PokrycieObj = new List<int>();
            bool lDotyczy;

            for (int obiekt = 0; obiekt < aSystemDecyzyjny.GetLength(0); obiekt++)
            {
                lDotyczy = true;
                foreach (Deskryptor deskryptor in this.deskryptory)
                {
                    if (aSystemDecyzyjny[obiekt, deskryptor.argument] != deskryptor.wartosc || aSystemDecyzyjny[obiekt, aSystemDecyzyjny.GetLength(1) - 1] != this.decyzja)
                    {
                        lDotyczy = false;
                        break;
                    }
                }

                if (lDotyczy)
                    PokrycieObj.Add(obiekt);
            }

            //pokrycie
            this.pokrycie = PokrycieObj.Count();
            //lista obiektow na ktorej wystepuje regula
            this.obiekty = PokrycieObj;

            return PokrycieObj;
        }

        public override string ToString()
        {
            string opis = "R" + this.numer + ": ";

            foreach (Deskryptor desc in this.deskryptory)
            {
                opis += "(a" + (desc.argument+1) + "=" + desc.wartosc + ")";

                if (desc != this.deskryptory.Last())
                    opis += "^";
            }

            opis += " => (d=" + this.decyzja + ")";

            if (this.pokrycie > 1)
            {
                opis += " [" + this.pokrycie + "]";
            }

            return opis;
        }
    }
}
