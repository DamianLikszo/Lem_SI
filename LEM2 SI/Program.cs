using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LEM2_SI
{
    class Program
    {
        static void Main(string[] args)
        {
            int[,] aSystemDecyzyjny;

            aSystemDecyzyjny = WczytajSystem();

            if (aSystemDecyzyjny != null)
            {
                Console.WriteLine("Wczytany System Decyzyjny: ");
                WypiszTablice(aSystemDecyzyjny);
                Console.WriteLine();

                GenerujRegulyLema(aSystemDecyzyjny);
            }

            Console.ReadKey();
            return;
        }

        static void GenerujRegulyLema(int[,] aSystemDecyzyjny)
        {
            List<int> listaDecyzji = GenerujListeDecyzji(aSystemDecyzyjny);
            List<Regula> listaRegol = new List<Regula>();

            // Przechodzimy przez wszystkie koncepty 
            foreach (int koncept in listaDecyzji)
            {
                bool lPokrytyKoncept = false;

                // petla bedze przebiegac az pokryjemy caly koncept, czyli az wygenerujemy reguly dla kazdego obiektu rozpatrywanego konceptu
                do
                {
                    bool lRegSprzeczna = false;
                    Regula regula = new Regula(koncept);
                    List<int> listaWyklArg = new List<int>();               // lista argumentow na ktorych mamy deskryptory
                    List<int> listaWyklObj = new List<int>();               // lista obiektow ktore sa wykluczone (sprawdzanie sprzecznosci) 

                    Deskryptor NajczestszyDesc;
                    Deskryptor PorownywanyDesc;

                    do           // rzezbimy regule dopoki nie jest sprzeczna 
                    {
                        // zerujemy na start
                        NajczestszyDesc = null;
                        PorownywanyDesc = null;

                        lRegSprzeczna = false;

                        //generujemy ktore obiekty sa do wykluczenia
                        listaWyklObj = new List<int>();     // resetujemy - sprzecznosc 
                        for (int obiekt = 0; obiekt < aSystemDecyzyjny.GetLength(0); obiekt++)
                        {
                            bool lPasuje = true;

                            // inny koncept
                            if (lPasuje && aSystemDecyzyjny[obiekt, aSystemDecyzyjny.GetLength(1) - 1] != koncept)
                            {
                                listaWyklObj.Add(obiekt);
                                lPasuje = false;
                            }

                            // wykluczamy obiekty ktorych nie dotyczy sprzeczna regula jezeli jest
                            if (lPasuje && regula.deskryptory.Count() > 0)
                            {
                                foreach (Deskryptor deskryptor in regula.deskryptory)
                                {
                                    if (aSystemDecyzyjny[obiekt, deskryptor.argument] != deskryptor.wartosc)
                                    {
                                        lPasuje = false;
                                        listaWyklObj.Add(obiekt);
                                        break;
                                    }
                                }
                            }

                            // obiekty z ktorych sa juz zbudowane reguly
                            if( lPasuje)
                            {
                                foreach (Regula Item in listaRegol)
                                {
                                    if (Item.obiekty.Contains(obiekt))
                                    {
                                        listaWyklObj.Add(obiekt);
                                        lPasuje = false;
                                        break;
                                    }
                                }
                            }

                            //wyjscie
                            if (!lPasuje)
                                continue;
                        }

                        for (int argument = 0; argument < aSystemDecyzyjny.GetLength(1)-1; argument++)
                        {
                            // wykluczamy argumenty na ktory juz sa wybrane deskryptory
                            if (listaWyklArg.Contains(argument))
                                continue;

                            for (int obiekt = 0; obiekt < aSystemDecyzyjny.GetLength(0); obiekt++)
                            {
                                // wykluczamy obiekty nalezace do listy obiektow wykluczonych 
                                if (listaWyklObj.Contains(obiekt))
                                    continue;

                                int nPokrycie = 0;          // ilosc wystapien
                                int wartosc = aSystemDecyzyjny[obiekt, argument];

                                //liczymy liczbe wystapien w kolumnie 
                                for (int wiersz = 0; wiersz < aSystemDecyzyjny.GetLength(0); wiersz++)
                                {
                                    if (listaWyklObj.Contains(wiersz))
                                        continue;

                                    if (aSystemDecyzyjny[wiersz, argument] == wartosc)
                                        nPokrycie++;
                                }

                                if (NajczestszyDesc == null)
                                {
                                    NajczestszyDesc = new Deskryptor(obiekt, argument, wartosc, nPokrycie);
                                }
                                else
                                {
                                    PorownywanyDesc = new Deskryptor(obiekt, argument, wartosc, nPokrycie);

                                    if (NajczestszyDesc.pokrycie < PorownywanyDesc.pokrycie)
                                    {
                                        NajczestszyDesc.obiekt = PorownywanyDesc.obiekt;
                                        NajczestszyDesc.argument = PorownywanyDesc.argument;
                                        NajczestszyDesc.wartosc = PorownywanyDesc.wartosc;
                                        NajczestszyDesc.pokrycie = PorownywanyDesc.pokrycie;
                                    }
                                }
                            }
                        }

                        // Dodajemy deskryptor do reguły
                        regula.Dodaj(NajczestszyDesc);
                        listaWyklArg.Add(NajczestszyDesc.argument);

                        // Sprawdzanie sprzecznosci reguly
                        bool lDotyczy; 

                        for (int obiekt = 0; obiekt < aSystemDecyzyjny.GetLength(0); obiekt++)
                        {
                            lDotyczy = true;

                            foreach (Deskryptor deskryptor in regula.deskryptory)
                            {
                                if (aSystemDecyzyjny[obiekt, deskryptor.argument] != deskryptor.wartosc)
                                {
                                    lDotyczy = false;
                                    break;
                                }
                            }

                            //regula w obiekcie i decyzja rozna to sprzeczne
                            if (lDotyczy && aSystemDecyzyjny[obiekt, aSystemDecyzyjny.GetLength(1) - 1] != koncept)
                            {
                                lRegSprzeczna = true;
                                break;
                            }  
                        }
                    } while (lRegSprzeczna);

                    //wyliczenie pokrycia reguly i dodanie reguly
                    regula.PokrycieObj(aSystemDecyzyjny);
                    listaRegol.Add(regula);

                    //czy caly koncept
                    int sumaObjKonceptu = 0;    // ile mamy obiektow na koncepcie
                    int sumaIleRegul = 0;       // ile mamy obiektow z ktorych zbudowalismy reguly w koncepcie

                    for (int wiersz = 0; wiersz < aSystemDecyzyjny.GetLength(0); wiersz++)
                    {
                        if (aSystemDecyzyjny[wiersz, aSystemDecyzyjny.GetLength(1)-1] == koncept)
                        {
                            sumaObjKonceptu++;
                        }
                    }

                    foreach (Regula item in listaRegol)
                    {
                        if (item.decyzja == koncept)
                        {
                            sumaIleRegul += item.pokrycie;
                        }
                    }

                    if (sumaObjKonceptu == sumaIleRegul)
                    {
                        lPokrytyKoncept = true;
                    }

                } while (!lPokrytyKoncept);
            }

            //Wypis Regul
            foreach (Regula item in listaRegol)
            {
                Console.WriteLine(item);
            }
            
            return;
        }

        // Generuje liste niepowtarzalnych decyzji wystepujacych w systemie decyzyjnym
        static List<int> GenerujListeDecyzji(int[,] aSystemDecyzyjny)
        {
            List<int> listaDecyzji = new List<int>();

            for (int wiersz = 0; wiersz < aSystemDecyzyjny.GetLength(0); wiersz++)
            {
                int decyzja = aSystemDecyzyjny[wiersz, aSystemDecyzyjny.GetLength(1) - 1];

                if (!listaDecyzji.Contains(decyzja))
                {
                    listaDecyzji.Add(decyzja);
                }
            }
            listaDecyzji.Sort();

            return listaDecyzji;
        }

        static int[,] WczytajSystem()
        {
            string cSciezkaDoPliku;
            string[] aLinie;
            int[,] aSystemDecyzyjny;
            int nCol, nRow;

            cSciezkaDoPliku = PobierzSciezke();

            if (cSciezkaDoPliku.Length > 0)
            {
                //Wczytanie zawartosci pliku
                aLinie = File.ReadAllLines(cSciezkaDoPliku);
                nCol = aLinie[0].Split(' ').Length;
                nRow = aLinie.Length;
                aSystemDecyzyjny = new int[nRow, nCol];

                for (int i = 0; i < aLinie.Length; i++)
                {
                    string[] aLiczby = aLinie[i].Split(' ');
                    for (int j = 0; j < aLiczby.Length; j++)
                    {
                        aSystemDecyzyjny[i, j] = int.Parse(aLiczby[j]);
                    }
                }
            }
            else
                aSystemDecyzyjny = null;

            return aSystemDecyzyjny;
        }

        static string PobierzSciezke()
        {
            string cFile;
            char input;
            bool lBreak = true;

            do
            {
                Console.Write("Podaj ścieżkę systemu decyzyjnego: ");
                //cFile = Console.ReadLine();

                cFile = "SystemDecyzyjny.txt";

                if (!File.Exists(cFile))
                {
                    cFile = String.Empty;
                    Console.WriteLine("Brak Pliku. Spróbować ponowanie? [T/N]");
                    input = Console.ReadKey().KeyChar;
                    lBreak = input != 't' && input != 'T';
                }

                Console.Clear();
            } while (!lBreak);

            return cFile;
        }

        static void WypiszTablice(int[,] aTab)
        {
            for (int i = 0; i < aTab.GetLength(0); i++)
            {
                for (int j = 0; j < aTab.GetLength(1); j++)
                {
                    Console.Write(aTab[i, j]);
                    Console.Write(' ');
                }
                Console.WriteLine();
            }
        }
    }
}