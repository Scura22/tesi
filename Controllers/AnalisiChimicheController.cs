using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
using RapportiLavoro.Global;
using RapportiLavoro.Models;
using RapportiLavoro.Models.Parameters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static RapportiLavoro.Global.Globals;

namespace RapportiLavoro.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AnalisiChimicheController : Controller
    {
        private readonly RFFContext _context;
        public bool bCalcolo = false;

        public AnalisiChimicheController(RFFContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ActionName("AnalisiFile")]
        public async Task<RetCode> AnalisiFile()
        {
            RetCode ret = new RetCode((int)Globals.RetCodes.None, "");
            List<string> files;

            try
            {

                /*#if DEBUG
                                if (Directory.Exists(@"C:\Analisi"))
                                    files = Directory.GetFiles(@"C:\Analisi", "*.csv", System.IO.SearchOption.TopDirectoryOnly).ToList();
                                else
                                    return;
                #else*/
                if (Directory.Exists(Globals.g_ChemicalPath))
                    files = Directory.GetFiles(Globals.g_ChemicalPath, "*.csv", System.IO.SearchOption.TopDirectoryOnly).ToList();
                else
                    return ret;
                //#endif

            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "RetrievingChemicalAnalisysFile");
                files = new List<string>();
            }

            foreach (string sFile in files)
            {
                if (System.IO.File.Exists(sFile))  //concorrenza
                {

                    TextFieldParser parser;
                    ElementiChimici[] materiaPrima;
                    DizStatiAnalisi[] phase;
                    ColateLegaSpecifica[] analisi;
                    ColateLegaBase[] analisiBase;
                    string[] fields;
                    string numColata, idColata = "", idMateriaPrima, valPercRilevata, newPath, idAnalisi = "", idFase = "9", idQuantometro;
                    bool isOk = false;
                    bool isBase = true;
                    bool perCliente = false;

                    newPath = sFile.Replace(sFile.Split('.')[sFile.Split('.').Length - 1], DateTime.Now.ToString("yyyyMMdd HHmmss.fff") + ".txt");
                    if (!System.IO.File.Exists(newPath))
                        System.IO.File.Copy(sFile, newPath);

                    System.IO.File.Move(sFile, newPath.Replace(".txt", ".old"));

                    parser = new TextFieldParser(newPath)
                    {
                        TextFieldType = FieldType.Delimited
                    };

                    parser.SetDelimiters(";");
                    fields = parser.ReadFields();

                    Globals.g_Logger.Info("||| File: " + sFile + " |||");

                    idQuantometro = fields[0];
                    numColata = fields[8];

                    if (numColata.Length == 5)
                    {
                        isBase = false;
                        isOk = true;
                    }
                    else if (numColata.Length == 8)
                    {
                        isBase = true;
                        isOk = true;
                    }

                    if (isOk)
                    {
                        if (isBase)
                        {
                            analisiBase = await GetScheduleBaseByNumber(numColata, true);

                            if (analisiBase != null && analisiBase.Length > 0)
                                idColata = analisiBase[0].ClbId.ToString();
                            else
                                Globals.g_Logger.Info("### Nessuna colata base con l'identificativo: " + numColata + " ###");
                        }
                        else
                        {
                            analisi = await GetScheduleSpecificaByNumber(numColata, true);

                            if (analisi != null && analisi.Length > 0)
                                idColata = analisi[0].ClsId.ToString();
                            else
                                Globals.g_Logger.Info("### Nessuna colata specifica con l'identificativo: " + numColata + " ###");
                        }
                        phase = await GetPhaseByName(fields[9]);

                        if (phase != null && phase.Length > 0)
                            idFase = phase[0].DsaId.ToString();
                        else
                        {
                            phase = await GetPhaseByName("ANALISI CHIMICHE");
                            idFase = phase[0].DsaId.ToString();
                        }

                        materiaPrima = await GetElementIdByCode(fields[15]);

                        if (materiaPrima != null && materiaPrima.Length > 0)
                        {
                            idMateriaPrima = materiaPrima[0].EcId.ToString();
                            valPercRilevata = fields[16];
                            ret = await InsertNewChemicalAnalysis(isBase, idColata, idMateriaPrima, valPercRilevata, idFase, DateTime.Now, "", fields[9], idQuantometro, perCliente);

                            if (ret.retCode == (int)Globals.RetCodes.Ok)
                            {
                                if (isBase)
                                {
                                    AnalisiChimicheBase acb = await GetNewChemicalAnalysisBase(idColata, idMateriaPrima, valPercRilevata);
                                    idAnalisi = acb.AcbId.ToString();
                                }
                                else
                                {
                                    AnalisiChimiche ac = await GetNewChemicalAnalysisSpecifiche(idColata, idMateriaPrima, valPercRilevata);
                                    idAnalisi = ac.AcId.ToString();
                                }

                            }

                            for (int i = 17; i < fields.Length - 2; i += 2)
                                try
                                {
                                    materiaPrima = await GetElementIdByCode(fields[i]);

                                    if (materiaPrima.Length == 0)
                                    {
                                        RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");
                                        Globals.g_Logger.Error("SavingChemicalData missing material: " + fields[i] + ".");
                                        Globals.g_Logger.Info("SavingChemicalData Insert missing material " + fields[i] + " with default value");

                                        ElementiChimici ec = new ElementiChimici() { EcCodice = fields[i], EcDescrizione = fields[i],
                                            EcDescrizioneAbbreviata = fields[i], EcFamiglia = null, EcFormato = "*", EcGruppoProdotto = 1,
                                            EcId = 0, EcImpurita = false, EcInUse = true, EcNome = fields[i], EcOrganico = 0, EcOssido = 0,
                                            EcPercentualeMetallo = 100, EcPercResaScoriaObiettivo = 100, EcPercScoriaObiettivo = 100,
                                            EcPezzatura = "*", EcProvenienza = 1, EcResa = 100, EcTipoMateriale = 4, EcUmidita = 0 };

                                        DictionaryController dic = new DictionaryController(_context);
                                        rc = await dic.InsertElementoChimico(ec);

                                        if (rc.retCode == (int)Globals.RetCodes.Ok)
                                            materiaPrima = await GetElementIdByCode(fields[i]);
                                        else
                                            Globals.g_Logger.Error("SavingChemicalData Errore nell'inserimento del nuovo elemento chimico: " + fields[i]);

                                    }

                                    if (materiaPrima != null && materiaPrima.Length > 0)
                                    {
                                        idMateriaPrima = materiaPrima[0].EcId.ToString();
                                        valPercRilevata = fields[i + 1];
                                        ret = await InsertNewChemicalAnalysis(isBase, idColata, idMateriaPrima, valPercRilevata, idFase, DateTime.Now, idAnalisi, fields[9], idQuantometro, perCliente);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Globals.g_Logger.Error(ex, "SavingChemicalData");
                                }
                        }
                        else
                            Globals.g_Logger.Error("SavingChemicalData missing material: " + fields[15]);
                    }
                }
            }

            return ret;
        }

        [HttpPost]
        [ActionName("GetChemicalForActiveAlloy")]
        public async Task<VAnalisiChimicheAllWeb[]> GetChemicalForActiveAlloy([FromBody] DizImpianti impianto)
        {
            List<VAnalisiChimicheAllWeb> ac = new List<VAnalisiChimicheAllWeb>();
            VAnalisiChimicheAllWeb[] ret;
            try
            {
                ac = await _context.VAnalisiChimicheAllWeb.Where(acaw => acaw.VacawDestinazione == impianto.DiCodice && acaw.VacawStatoColata == 3).ToListAsync();

                ac = ac.GroupBy(acaw => new { acaw.VacawId, acaw.VacawNomeStato }).Select(acaw => acaw.First()).ToList();

                int rowNumber = 1;

                ac.ForEach( analisi => {
                    analisi.VacawRowNumber = rowNumber;
                    rowNumber++;
                });

                ret = ac.OrderByDescending(analisi => analisi.VacawData).ToArray();

            }
            catch (Exception ex)
            {
                ret = null;
                Globals.g_Logger.Error(ex, "GetChemicalForActiveAlloy");
            }
            return ret;
        }

        [HttpPost]
        [ActionName("GetChemicalForColata")]
        public async Task<VAnalisiChimicheAllWeb[]> GetChemicalForColata([FromBody] VAnalisiChimicheAllWeb param)
        {
            List<VAnalisiChimicheAllWeb> ac = new List<VAnalisiChimicheAllWeb>();
            VAnalisiChimicheAllWeb[] ret;
            try
            {
                ac = await _context.VAnalisiChimicheAllWeb.Where(acaw => acaw.VacawIdColata == param.VacawIdColata && acaw.VacawDestinazione == param.VacawDestinazione).ToListAsync();

                ac = ac.GroupBy(acaw => new { acaw.VacawId, acaw.VacawNomeStato }).Select(acaw => acaw.First()).ToList();

                int rowNumber = 1;

                ac.ForEach(analisi => {
                    analisi.VacawRowNumber = rowNumber;
                    rowNumber++;
                });

                ret = ac.OrderByDescending(analisi => analisi.VacawData).ToArray();

            }
            catch (Exception ex)
            {
                ret = null;
                Globals.g_Logger.Error(ex, "GetChemicalForColata");
            }
            return ret;
        }


        [HttpPost]
        [ActionName("GetElencoAnalisi")]
        public async Task<ElencoAnalisi> GetElencoAnalisi([FromBody] ElencoAnalisi ea)
        {
            List<RffRilevazione> r = new List<RffRilevazione>();
            List<RffCorrezione> cm = new List<RffCorrezione>();
            List<RffCorrezione> c = new List<RffCorrezione>();
            bCalcolo = true;
            ea.PesoCorretto = ea.Peso;

            object[] percMetallo, objCorrezioni;
            decimal? dWeight, percRilevata;
            string codMateria;
            VAnalisiChimicheAllWeb[] dtAnalisi;

            dtAnalisi = await GetChemicalByID(ea.IdImpianto, ea.IdAnalisi);

            foreach (VAnalisiChimicheAllWeb da in dtAnalisi)
            {
                percMetallo = new object[] { 0, 0, 0, 0, 0, 0, 0 };
                codMateria = da.VacawCodiceMateriaPrima;

                if (codMateria == "Al04_0")
                    continue;

                try { percMetallo = await ElencoPercentuali(codMateria, ea.IdColata, ea.IdImpianto); }
                catch (Exception ex)
                {
                    percMetallo = new object[] { 0, 0, 0, 0, 0, 0, 0 };
                    Globals.g_Logger.Error(ex, ex.Message.ToString());
                }

                percRilevata = Convert.ToDecimal(da.VacawPercRilevata); //recupero percentuale rilevata
                percMetallo = new object[] { codMateria, percRilevata }.Concat(percMetallo).ToArray(); //concateno codice materia prima, percentuale rilevataq con risultato query percentuali richieste
                Array.Resize(ref percMetallo, percMetallo.Length + 1);

                if (ea.Peso.HasValue)
                    dWeight = (percRilevata * ea.Peso.Value / 100);
                else
                    dWeight = null;
                percMetallo[percMetallo.Length - 1] = dWeight;
                RffRilevazione newR = new RffRilevazione();
                newR.Elemento = percMetallo[0].ToString();
                newR.PercRilevata = Convert.ToDecimal(percMetallo[1]);
                newR.PercNominale = Convert.ToDecimal(percMetallo[2]);
                newR.PercMin = Convert.ToDecimal(percMetallo[3]);
                newR.PercMax = Convert.ToDecimal(percMetallo[4]);
                newR.PercMinCliente = Convert.ToDecimal(percMetallo[5]);
                newR.PercMaxCliente = Convert.ToDecimal(percMetallo[6]);
                newR.PercMinRaffmetal = Convert.ToDecimal(percMetallo[7]);
                newR.PercMaxRaffmetal = Convert.ToDecimal(percMetallo[8]);
                newR.Peso = Convert.ToDecimal(percMetallo[9]);
                r.Add(newR);

                if (ea.Peso.HasValue)
                {
                    objCorrezioni = new object[] { codMateria, percRilevata, percRilevata, dWeight, dWeight, 0 };
                    RffCorrezione newC = new RffCorrezione();
                    newC.Elemento = objCorrezioni[0].ToString();
                    newC.PercIniziale = Convert.ToDecimal(objCorrezioni[1]);
                    newC.PercFinale = Convert.ToDecimal(objCorrezioni[2]);
                    newC.PesoIniziale = Convert.ToDecimal(objCorrezioni[3]);
                    newC.PesoFinale = Convert.ToDecimal(objCorrezioni[4]);
                    newC.QtaDaCaricare = Convert.ToDecimal(objCorrezioni[5]);
                    c.Add(newC);
                }
            }

            ea.Rilevazioni = r.ToArray();

            VCorrezioni[] vc = await _context.VCorrezioni.Where(c => c.VcIdAnalisi == ea.IdAnalisi).ToArrayAsync();
            decimal pesoTot = PesoTotaleColata(ea.Rilevazioni) + PesoTotaleColataCorretto(vc);

            foreach (VCorrezioni correzione in vc)
            {
                RffCorrezione corrM = new RffCorrezione();
                corrM.DescBreve = correzione.VcDescrizioneBreve;
                corrM.Elemento = correzione.VcCodiceMateria;
                corrM.IdMateriaCorrettivo = correzione.VcIdMateria;
                corrM.QtaDaCaricare = Convert.ToDecimal(correzione.VcQtaDaCaricare);
                corrM.QtaCaricata = Convert.ToDecimal(correzione.VcQtaCaricata);
                cm.Add(corrM);
                VElementiChimiciMateriePrime[] elementiCorrettivo = await _context.VElementiChimiciMateriePrime.Where(ecmp => ecmp.VecmpIdMateriaPrima == corrM.IdMateriaCorrettivo).ToArrayAsync();
                foreach (VElementiChimiciMateriePrime elemento in elementiCorrettivo)
                {
                    RffCorrezione actualElemento = c.Where(co => co.Elemento == elemento.VecmpCodiceElementoChimico).FirstOrDefault();

                    if (actualElemento != null)
                    {
                        actualElemento.QtaDaCaricare += Convert.ToDecimal(correzione.VcQtaDaCaricare) * Convert.ToDecimal(elemento.VecmpPercentuale) / 100;
                        actualElemento.PesoFinale = actualElemento.PesoIniziale + actualElemento.QtaDaCaricare;
                        actualElemento.PercFinale = actualElemento.PesoFinale * 100 / pesoTot;
                    }
                    else
                    {

                        RffCorrezione corr = new RffCorrezione();
                        corr.DescBreve = elemento.VecmpDescrizioneBreveElementoChimico;
                        corr.Elemento = elemento.VecmpCodiceElementoChimico;
                        corr.QtaDaCaricare = Convert.ToDecimal(correzione.VcQtaDaCaricare) * Convert.ToDecimal(elemento.VecmpPercentuale) / 100;
                        actualElemento.PesoFinale = actualElemento.PesoIniziale + actualElemento.QtaDaCaricare;
                        actualElemento.PercFinale = actualElemento.PesoFinale * 100 / pesoTot;
                        c.Add(corr);
                    }
                }
            }

            ea.CorrezioniMaterie = cm.ToArray();
            ea.Correzioni = c.ToArray();

            bCalcolo = false;

            return ea;
        }

        [HttpPost]
        [ActionName("CalcoloPropostaCorrezioni")]
        public async Task<ElencoAnalisi> CalcoloPropostaCorrezioni([FromBody] ElencoAnalisi ea)
        {
            bCalcolo = true;
            ea = await GetElencoAnalisi(ea);

            decimal dPerElem = 0, dPerConfr = 0, dPerNor = 0;
            decimal dPesoTotElem = 0, dPesoTotColata = 0;
            int iPMin5 = 7, cont = 0, dtRowNumber;

            dtRowNumber = ea.Rilevazioni.Length;

            //passo della correzione
            decimal dPasso = Convert.ToDecimal(0.01);

            dPesoTotColata = PesoTotaleColata(ea.Rilevazioni);

        inizio:

            for (int i = 0; i < dtRowNumber; i++)
            {
                //recupero la pec minima e mmassima e della normativa dell'elemento
                dPerNor = Convert.ToDecimal(ea.Rilevazioni[i].PercNominale);

                //se la percentuale nominale desiderata è =0 allora salto l'esecuzione 
                if (dPerNor == 0)
                    continue;

                //salvo la percentuale attuale dell'elemento in realzione al peso totale e il suo peso
                dPesoTotElem = Convert.ToDecimal(ea.Rilevazioni[i].Peso);
                dPerElem = (dPesoTotElem / dPesoTotColata) * 100;

                // imposto la percentuale desiderata = a quella della normativa
                dPerConfr = dPerNor;

                //se la percentuale sul nuovo totale è < della percentuale desiderata
                while (dPerElem < dPerConfr)
                {
                    //aumento peso totale e peso del singolo elemento della stessa quantittà
                    dPesoTotColata += dPasso;
                    dPesoTotElem += dPasso;
                    dPerElem = (dPesoTotElem / dPesoTotColata) * 100;
                }

                //salvo le modifiche di percentuale e peso nella tabella di appoggio
                ea.Rilevazioni[i].PercRilevata = dPerElem;
                ea.Rilevazioni[i].Peso = dPesoTotElem;
            }

            //calcolo nuova percentuale in base all'aumento del peso per tutti gli elementi
            for (int k = 0; k < dtRowNumber; k++)
            {
                dPesoTotElem = Convert.ToDecimal(ea.Rilevazioni[k].Peso);

                //se il peso è 0 non calcolo la percentuale...
                if (dPesoTotElem == 0)
                    continue;

                //calcolo percentuale per tutti gli elementi sul peso finale definitivo
                ea.Rilevazioni[k].PercRilevata = (dPesoTotElem / dPesoTotColata) * 100;
                dPerElem = Convert.ToDecimal(ea.Rilevazioni[k].PercRilevata);
            }

            //se qualche elemento non rispetta i parametri di minimo e massimo desiderati allora continuo i calcoli
            for (int k = 0; k < dtRowNumber; k++)
            {
                dPerElem = Convert.ToDecimal(ea.Rilevazioni[k].PercRilevata);
                dPerNor = Convert.ToDecimal(ea.Rilevazioni[k].PercNominale);

                if (dPerElem < dPerNor)
                {
                    cont++;
                    goto inizio;
                }
            }

            //per ogni riga vado a ricalcolare il totale a seguto delle modifiche della riga precedente
            dPesoTotColata = PesoTotaleColata(ea.Rilevazioni);

            // aggiorno la tabella correzioni
            for (int x = 0; x < dtRowNumber; x++)
            {
                for (int y = 0; y < ea.Correzioni.Length; y++)
                {
                    if (ea.Correzioni[y].Elemento == ea.Rilevazioni[x].Elemento)
                    {
                        ea.Correzioni[y].PercFinale = Decimal.Round(Convert.ToDecimal(ea.Rilevazioni[x].PercRilevata), 4);
                        ea.Correzioni[y].PesoFinale = Convert.ToDecimal(ea.Rilevazioni[x].Peso);
                        ea.Correzioni[y].QtaDaCaricare = Convert.ToDecimal(ea.Correzioni[y].PesoFinale) - Convert.ToDecimal(ea.Correzioni[y].PesoIniziale);
                    }
                }
            }

            Globals.g_CorrezioniAppoggio = ea.Correzioni;

            ea.PesoCorretto = dPesoTotColata;

            bCalcolo = false;
            return ea;
        }

        [HttpPost]
        [ActionName("CalcoloPropostaCorrezioniMaterie")]
        public async Task<ElencoAnalisi> CalcoloPropostaCorrezioniMaterie([FromBody] ElencoAnalisi ea)
        {

            bCalcolo = true;

            ea = await GetElencoAnalisi(ea);

            decimal dPerElem = 0, dPerConfr = 0, dPerNor = 0;
            decimal dPesoTotElem = 0, dPesoTotColata = 0;
            int iPMin5 = 7, cont = 0, dtRowNumber;

            VElementiChimiciMateriePrime[] elementiCorrettivo;

            //ea.CorrezioniMaterie = new RffCorrezione[0];
            List<RffCorrezione[]> CorrezioniMaterie = new List<RffCorrezione[]>();
            List<RffCorrezione[]> CorrezioniMaterieAppoggio = new List<RffCorrezione[]>();
            List<RffCorrezione[]> Correzioni = new List<RffCorrezione[]>();
            List<RffRilevazione[]> Rilevazioni = new List<RffRilevazione[]>();
            List<decimal> PesoCorrettoList = new List<decimal>();
            dtRowNumber = ea.Rilevazioni.Length;

            //passo della correzione
            decimal dPasso = Convert.ToDecimal(0.01);

            dPesoTotColata = PesoTotaleColata(ea.Rilevazioni);


            for (int i = 0; i < dtRowNumber; i++)
            {
                List<VElementiChimiciMateriePrime> temp = await _context.VElementiChimiciMateriePrime.Where(ecmp => ecmp.VecmpCodiceElementoChimico == ea.Rilevazioni[i].Elemento && ecmp.VecmpElementoACorrettivo && ecmp.VecmpInUse).ToListAsync();

                temp.ForEach(t =>
                {

                    RffCorrezione c = new RffCorrezione();
                    c.Elemento = t.VecmpCodiceMateriaPrima;
                    c.IdMateriaCorrettivo = t.VecmpIdMateriaPrima;
                    c.DescBreve = t.VecmpDescrizioneBreveMateriaPrima;
                    c.PesoIniziale = 0;
                    c.PesoFinale = 0;
                    c.QtaCaricata = 0;
                    c.ElementoChimico = ea.Rilevazioni[i].Elemento;

                    RffCorrezione[] correzione = new RffCorrezione[] { c };

                    CorrezioniMaterie.Add(correzione);
                });

            }

            //ea.PesoCorrettoMatrix = new decimal[CorrezioniMaterie.Count];

            //int count = 0;

            CorrezioniMaterie.ForEach(correzioniMaterie =>
            {
                dPesoTotColata = PesoTotaleColata(ea.Rilevazioni);

                RffCorrezione[] correzioni = new RffCorrezione[ea.Correzioni.Length];
                RffRilevazione[] rilevazioni = new RffRilevazione[ea.Rilevazioni.Length];

                for (int i = 0; i < correzioni.Length; i++)
                {
                    correzioni[i] = new RffCorrezione();
                    correzioni[i].DescBreve = ea.Correzioni[i].DescBreve;
                    correzioni[i].Elemento = ea.Correzioni[i].Elemento;
                    correzioni[i].IdMateriaCorrettivo = ea.Correzioni[i].IdMateriaCorrettivo;
                    correzioni[i].PercFinale = ea.Correzioni[i].PercFinale;
                    correzioni[i].PercIniziale = ea.Correzioni[i].PercIniziale;
                    correzioni[i].PesoFinale = ea.Correzioni[i].PesoFinale;
                    correzioni[i].PesoIniziale = ea.Correzioni[i].PesoIniziale;
                    correzioni[i].QtaCaricata = ea.Correzioni[i].QtaCaricata;
                    correzioni[i].QtaDaCaricare = ea.Correzioni[i].QtaDaCaricare;
                }
                
                for (int i = 0; i < rilevazioni.Length; i++)
                {
                    rilevazioni[i] = new RffRilevazione();
                    rilevazioni[i].Elemento = ea.Rilevazioni[i].Elemento;
                    rilevazioni[i].PercMax = ea.Rilevazioni[i].PercMax;
                    rilevazioni[i].PercMaxCliente = ea.Rilevazioni[i].PercMaxCliente;
                    rilevazioni[i].PercMaxRaffmetal = ea.Rilevazioni[i].PercMaxRaffmetal;
                    rilevazioni[i].PercMin = ea.Rilevazioni[i].PercMin;
                    rilevazioni[i].PercMinCliente = ea.Rilevazioni[i].PercMinCliente;
                    rilevazioni[i].PercMinRaffmetal = ea.Rilevazioni[i].PercMinRaffmetal;
                    rilevazioni[i].PercNominale = ea.Rilevazioni[i].PercNominale;
                    rilevazioni[i].PercRilevata = ea.Rilevazioni[i].PercRilevata;
                    rilevazioni[i].Peso = ea.Rilevazioni[i].Peso;
                }


            inizio:

                for (int i = 0; i < dtRowNumber; i++)
                {
                    VElementiChimiciMateriePrime correttivoTmp;

                    if (rilevazioni[i].Elemento == correzioniMaterie.Last().ElementoChimico)
                        correttivoTmp = _context.VElementiChimiciMateriePrime.Where(ecmp => ecmp.VecmpIdMateriaPrima == correzioniMaterie.Last().IdMateriaCorrettivo && ecmp.VecmpElementoACorrettivo && ecmp.VecmpInUse).FirstOrDefault();
                    else
                        correttivoTmp = _context.VElementiChimiciMateriePrime.Where(ecmp => ecmp.VecmpCodiceElementoChimico == rilevazioni[i].Elemento && ecmp.VecmpElementoACorrettivo && ecmp.VecmpInUse).FirstOrDefault();
                    if (correttivoTmp != null)
                    {
                        int idMateria = correttivoTmp.VecmpIdMateriaPrima;
                        string codMateria = correttivoTmp.VecmpCodiceMateriaPrima;
                        string descBreve = correttivoTmp.VecmpDescrizioneBreveMateriaPrima;
                        elementiCorrettivo = _context.VElementiChimiciMateriePrime.Where(ecmp => ecmp.VecmpIdMateriaPrima == idMateria).ToArray();

                        if (correzioniMaterie.Where(cm => cm.IdMateriaCorrettivo == idMateria).Count() == 0)
                        {
                            RffCorrezione c = new RffCorrezione();
                            c.IdMateriaCorrettivo = idMateria;
                            c.Elemento = codMateria;
                            c.DescBreve = descBreve;
                            c.PesoIniziale = 0;
                            c.PesoFinale = 0;
                            c.QtaDaCaricare = 0;

                            correzioniMaterie = new RffCorrezione[] { c }.Concat(correzioniMaterie).ToArray();
                        }

                        //recupero la pec minima e mmassima e della normativa dell'elemento
                        dPerNor = Convert.ToDecimal(rilevazioni[i].PercNominale);

                        //se la percentuale nominale desiderata è =0 allora salto l'esecuzione 
                        if (dPerNor == 0)
                            continue;

                        //salvo la percentuale attuale dell'elemento in realzione al peso totale e il suo peso
                        dPesoTotElem = Convert.ToDecimal(rilevazioni[i].Peso);
                        dPerElem = (dPesoTotElem / dPesoTotColata) * 100;

                        // imposto la percentuale desiderata = a quella della normativa
                        dPerConfr = dPerNor;

                        //se la percentuale sul nuovo totale è < della percentuale desiderata
                        while (dPerElem < dPerConfr)
                        {
                            //aumento peso totale e peso del singolo elemento della stessa quantittà
                            dPesoTotColata += dPasso; //aumento il peso totale della colata perchè aggiungo dPasso qta del correttivo
                            correzioniMaterie.Where(cm => cm.IdMateriaCorrettivo == idMateria).First().PesoFinale += dPasso; //incremento il peso totale del correttivo
                            correzioniMaterie.Where(cm => cm.IdMateriaCorrettivo == idMateria).First().QtaDaCaricare = correzioniMaterie.Where(cm => cm.IdMateriaCorrettivo == idMateria).First().PesoFinale +
                                correzioniMaterie.Where(cm => cm.IdMateriaCorrettivo == idMateria).First().PesoIniziale; //ricalcolo la quantità da caricare del correttivo
                            dPesoTotElem += dPasso * Convert.ToDecimal(elementiCorrettivo.Where(ec => ec.VecmpCodiceElementoChimico == rilevazioni[i].Elemento).First().VecmpPercentuale) / 100; //aumento il peso dell'elemento in proporzione alla percentuale nel correttivo
                                                                                                                                                                                                 //incremento la quantità in relazione alla percentuale nel correttivo degli altri elementi presenti nel correttivo
                            for (int k = 0; k < elementiCorrettivo.Count(); k++)
                            {
                                if (elementiCorrettivo[k].VecmpCodiceElementoChimico != rilevazioni[i].Elemento)
                                {
                                    rilevazioni.Where(r => r.Elemento == elementiCorrettivo[k].VecmpCodiceElementoChimico).First().Peso += dPasso * Convert.ToDecimal(elementiCorrettivo[k].VecmpPercentuale) / 100;
                                }
                            }

                            dPerElem = (dPesoTotElem / dPesoTotColata) * 100;
                        }

                        //salvo le modifiche di percentuale e peso nella tabella di appoggio
                        rilevazioni[i].PercRilevata = dPerElem;
                        rilevazioni[i].Peso = dPesoTotElem;
                    }
                }

                //calcolo nuova percentuale in base all'aumento del peso per tutti gli elementi
                for (int k = 0; k < dtRowNumber; k++)
                {
                    dPesoTotElem = Convert.ToDecimal(rilevazioni[k].Peso);

                    //se il peso è 0 non calcolo la percentuale...
                    if (dPesoTotElem == 0)
                        continue;

                    //calcolo percentuale per tutti gli elementi sul peso finale definitivo
                    rilevazioni[k].PercRilevata = (dPesoTotElem / dPesoTotColata) * 100;
                    dPerElem = Convert.ToDecimal(rilevazioni[k].PercRilevata);
                }

                //se qualche elemento non rispetta i parametri di minimo e massimo desiderati allora continuo i calcoli
                for (int k = 0; k < dtRowNumber; k++)
                {
                    dPerElem = Convert.ToDecimal(rilevazioni[k].PercRilevata);
                    dPerNor = Convert.ToDecimal(rilevazioni[k].PercNominale);

                    if (dPerElem < dPerNor)
                    {
                        cont++;
                        goto inizio;
                    }
                }

                //per ogni riga vado a ricalcolare il totale a seguto delle modifiche della riga precedente
                dPesoTotColata = PesoTotaleColata(rilevazioni);

                // aggiorno la tabella correzioni
                for (int x = 0; x < dtRowNumber; x++)
                {
                    for (int y = 0; y < correzioni.Length; y++)
                    {
                        if (correzioni[y].Elemento == rilevazioni[x].Elemento)
                        {
                            correzioni[y].PercFinale = Decimal.Round(Convert.ToDecimal(rilevazioni[x].PercRilevata), 4);
                            correzioni[y].PesoFinale = Convert.ToDecimal(rilevazioni[x].Peso);
                            correzioni[y].QtaDaCaricare = Convert.ToDecimal(correzioni[y].PesoFinale) - Convert.ToDecimal(correzioni[y].PesoIniziale);
                        }
                    }
                }

                Globals.g_CorrezioniAppoggio = correzioni;

                if (correzioniMaterie.Last().QtaDaCaricare != 0)
                {

                    PesoCorrettoList.Add(dPesoTotColata);

                    Rilevazioni.Add(rilevazioni);
                    Correzioni.Add(correzioni);

                    CorrezioniMaterieAppoggio.Add(correzioniMaterie.Reverse().ToArray());
                }

                //count++;

            });

            ea.PesoCorrettoMatrix = PesoCorrettoList.ToArray();
            ea.CorrezioniMaterieMatrix = CorrezioniMaterieAppoggio.ToArray();
            ea.CorrezioniMatrix = Correzioni.ToArray();
            ea.RilevazioniMatrix = Rilevazioni.ToArray();

            bCalcolo = false;
            return ea;
        }


        //[HttpPost]
        //[ActionName("CalcoloPropostaCorrezioniMaterie")]
        //public async Task<ElencoAnalisi> CalcoloPropostaCorrezioniMaterie([FromBody] ElencoAnalisi ea)
        //{

        //    bCalcolo = true;

        //    ea = await GetElencoAnalisi(ea);

        //    decimal dPerElem = 0, dPerConfr = 0, dPerNor = 0;
        //    decimal dPesoTotElem = 0, dPesoTotColata = 0;
        //    int iPMin5 = 7, cont = 0, dtRowNumber;

        //    VElementiChimiciMateriePrime[] elementiCorrettivo;

        //    ea.CorrezioniMaterie = new RffCorrezione[0];

        //    dtRowNumber = ea.Rilevazioni.Length;

        //    //passo della correzione
        //    decimal dPasso = Convert.ToDecimal(0.01);

        //    dPesoTotColata = PesoTotaleColata(ea.Rilevazioni);

        //inizio:

        //    for (int i = 0; i < dtRowNumber; i++)
        //    {
        //        VElementiChimiciMateriePrime correttivoTmp = await _context.VElementiChimiciMateriePrime.Where(ecmp => ecmp.VecmpCodiceElementoChimico == ea.Rilevazioni[i].Elemento && ecmp.VecmpElementoACorrettivo && ecmp.VecmpInUse).FirstOrDefaultAsync();
        //        if (correttivoTmp != null)
        //        {
        //            int idMateria = correttivoTmp.VecmpIdMateriaPrima;
        //            string codMateria = correttivoTmp.VecmpCodiceMateriaPrima;
        //            string descBreve = correttivoTmp.VecmpDescrizioneBreveMateriaPrima;
        //            elementiCorrettivo = await _context.VElementiChimiciMateriePrime.Where(ecmp => ecmp.VecmpIdMateriaPrima == idMateria).ToArrayAsync();

        //            if (ea.CorrezioniMaterie.Where(cm => cm.IdMateriaCorrettivo == idMateria).Count() == 0)
        //            {
        //                RffCorrezione c = new RffCorrezione();
        //                c.IdMateriaCorrettivo = idMateria;
        //                c.Elemento = codMateria;
        //                c.DescBreve = descBreve;
        //                c.PesoIniziale = 0;
        //                c.PesoFinale = 0;
        //                c.QtaDaCaricare = 0;

        //                ea.CorrezioniMaterie = new RffCorrezione[] { c }.Concat(ea.CorrezioniMaterie).ToArray();
        //            }

        //            //recupero la pec minima e mmassima e della normativa dell'elemento
        //            dPerNor = Convert.ToDecimal(ea.Rilevazioni[i].PercNominale);

        //            //se la percentuale nominale desiderata è =0 allora salto l'esecuzione 
        //            if (dPerNor == 0)
        //                continue;

        //            //salvo la percentuale attuale dell'elemento in realzione al peso totale e il suo peso
        //            dPesoTotElem = Convert.ToDecimal(ea.Rilevazioni[i].Peso);
        //            dPerElem = (dPesoTotElem / dPesoTotColata) * 100;

        //            // imposto la percentuale desiderata = a quella della normativa
        //            dPerConfr = dPerNor;

        //            //se la percentuale sul nuovo totale è < della percentuale desiderata
        //            while (dPerElem < dPerConfr)
        //            {
        //                //aumento peso totale e peso del singolo elemento della stessa quantittà
        //                dPesoTotColata += dPasso; //aumento il peso totale della colata perchè aggiungo dPasso qta del correttivo
        //                ea.CorrezioniMaterie.Where(cm => cm.IdMateriaCorrettivo == idMateria).First().PesoFinale += dPasso; //incremento il peso totale del correttivo
        //                ea.CorrezioniMaterie.Where(cm => cm.IdMateriaCorrettivo == idMateria).First().QtaDaCaricare = ea.CorrezioniMaterie.Where(cm => cm.IdMateriaCorrettivo == idMateria).First().PesoFinale +
        //                    ea.CorrezioniMaterie.Where(cm => cm.IdMateriaCorrettivo == idMateria).First().PesoIniziale; //ricalcolo la quantità da caricare del correttivo
        //                dPesoTotElem += dPasso * Convert.ToDecimal(elementiCorrettivo.Where(ec => ec.VecmpCodiceElementoChimico == ea.Rilevazioni[i].Elemento).First().VecmpPercentuale) / 100; //aumento il peso dell'elemento in proporzione alla percentuale nel correttivo
        //                                                                                                                                                                                        //incremento la quantità in relazione alla percentuale nel correttivo degli altri elementi presenti nel correttivo
        //                for (int k = 0; k < elementiCorrettivo.Count(); k++)
        //                {
        //                    if (elementiCorrettivo[k].VecmpCodiceElementoChimico != ea.Rilevazioni[i].Elemento)
        //                    {
        //                        ea.Rilevazioni.Where(r => r.Elemento == elementiCorrettivo[k].VecmpCodiceElementoChimico).First().Peso += dPasso * Convert.ToDecimal(elementiCorrettivo[k].VecmpPercentuale) / 100;
        //                    }
        //                }

        //                dPerElem = (dPesoTotElem / dPesoTotColata) * 100;
        //            }

        //            //salvo le modifiche di percentuale e peso nella tabella di appoggio
        //            ea.Rilevazioni[i].PercRilevata = dPerElem;
        //            ea.Rilevazioni[i].Peso = dPesoTotElem;
        //        }
        //    }

        //    //calcolo nuova percentuale in base all'aumento del peso per tutti gli elementi
        //    for (int k = 0; k < dtRowNumber; k++)
        //    {
        //        dPesoTotElem = Convert.ToDecimal(ea.Rilevazioni[k].Peso);

        //        //se il peso è 0 non calcolo la percentuale...
        //        if (dPesoTotElem == 0)
        //            continue;

        //        //calcolo percentuale per tutti gli elementi sul peso finale definitivo
        //        ea.Rilevazioni[k].PercRilevata = (dPesoTotElem / dPesoTotColata) * 100;
        //        dPerElem = Convert.ToDecimal(ea.Rilevazioni[k].PercRilevata);
        //    }

        //    //se qualche elemento non rispetta i parametri di minimo e massimo desiderati allora continuo i calcoli
        //    for (int k = 0; k < dtRowNumber; k++)
        //    {
        //        dPerElem = Convert.ToDecimal(ea.Rilevazioni[k].PercRilevata);
        //        dPerNor = Convert.ToDecimal(ea.Rilevazioni[k].PercNominale);

        //        if (dPerElem < dPerNor)
        //        {
        //            cont++;
        //            goto inizio;
        //        }
        //    }

        //    //per ogni riga vado a ricalcolare il totale a seguto delle modifiche della riga precedente
        //    dPesoTotColata = PesoTotaleColata(ea.Rilevazioni);

        //    // aggiorno la tabella correzioni
        //    for (int x = 0; x < dtRowNumber; x++)
        //    {
        //        for (int y = 0; y < ea.Correzioni.Length; y++)
        //        {
        //            if (ea.Correzioni[y].Elemento == ea.Rilevazioni[x].Elemento)
        //            {
        //                ea.Correzioni[y].PercFinale = Decimal.Round(Convert.ToDecimal(ea.Rilevazioni[x].PercRilevata), 4);
        //                ea.Correzioni[y].PesoFinale = Convert.ToDecimal(ea.Rilevazioni[x].Peso);
        //                ea.Correzioni[y].QtaDaCaricare = Convert.ToDecimal(ea.Correzioni[y].PesoFinale) - Convert.ToDecimal(ea.Correzioni[y].PesoIniziale);
        //            }
        //        }
        //    }

        //    Globals.g_CorrezioniAppoggio = ea.Correzioni;

        //    ea.PesoCorretto = dPesoTotColata;

        //    bCalcolo = false;
        //    return ea;
        //}


        [HttpPost]
        [ActionName("RicalcolaModificaOperatore")]
        public async Task<ElencoAnalisi> RicalcolaModificaOperatore([FromBody] ElencoAnalisi ea)
        {
            if (!bCalcolo)
            {
                bCalcolo = true;
                decimal pNewTot = 0;

                for (int i = 0; i < ea.Correzioni.Length; i++)
                    pNewTot = pNewTot + Convert.ToDecimal(ea.Correzioni[i].PesoIniziale) + Convert.ToDecimal(ea.Correzioni[i].QtaDaCaricare);


                for (int i = 0; i < ea.Correzioni.Length; i++)
                {
                    ea.Correzioni[i].PesoFinale = Convert.ToDecimal(ea.Correzioni[i].QtaDaCaricare) + Convert.ToDecimal(ea.Correzioni[i].PesoIniziale);
                    ea.Correzioni[i].PercFinale = Convert.ToDecimal(Convert.ToDecimal(ea.Correzioni[i].PesoFinale) * 100 / pNewTot);
                }

                bCalcolo = false;

            }

            return ea;

        }

        [HttpPost]
        [ActionName("RicalcolaModificaOperatoreMaterie")]
        public async Task<ElencoAnalisi> RicalcolaModificaOperatoreMaterie([FromBody] ElencoAnalisi ea)
        {
            if (!bCalcolo)
            {
                bCalcolo = true;
                decimal pNewTot = ea.PesoCorretto.Value;

                //for (int i = 0; i < ea.Correzioni.Length; i++)
                //    pNewTot = pNewTot + Convert.ToDecimal(ea.Correzioni[i].PesoIniziale) + Convert.ToDecimal(ea.Correzioni[i].QtaDaCaricare);

                foreach (RffCorrezione c in ea.Correzioni)
                {
                    c.QtaDaCaricare = 0;
                }

                foreach (RffCorrezione cm in ea.CorrezioniMaterie)
                {

                    VElementiChimiciMateriePrime correttivoTmp = await _context.VElementiChimiciMateriePrime.Where(ecmp => ecmp.VecmpCodiceMateriaPrima == cm.Elemento && ecmp.VecmpElementoACorrettivo && ecmp.VecmpInUse).FirstOrDefaultAsync();
                    if (correttivoTmp != null)
                    {
                        int idMateria = correttivoTmp.VecmpIdMateriaPrima;
                        VElementiChimiciMateriePrime[] elementiCorrettivo = await _context.VElementiChimiciMateriePrime.Where(ecmp => ecmp.VecmpIdMateriaPrima == idMateria).ToArrayAsync();

                        foreach (VElementiChimiciMateriePrime elemento in elementiCorrettivo)
                        {
                            bool elementoNull = false;
                            RffCorrezione actualElemento = ea.Correzioni.Where(c => c.Elemento == elemento.VecmpCodiceElementoChimico).FirstOrDefault();

                            if (actualElemento == null)
                            {
                                elementoNull = true;
                                actualElemento = new RffCorrezione();
                                actualElemento.QtaDaCaricare = 0;
                                actualElemento.PercIniziale = 0;
                                actualElemento.PesoIniziale = 0;
                                actualElemento.IdMateriaCorrettivo = elemento.VecmpIdMateriaPrima;
                                actualElemento.QtaCaricata = 0;
                                actualElemento.DescBreve = elemento.VecmpDescrizioneBreveElementoChimico;
                                actualElemento.Elemento = elemento.VecmpCodiceElementoChimico;
                            }

                            actualElemento.QtaDaCaricare += cm.QtaDaCaricare * Convert.ToDecimal(elemento.VecmpPercentuale) / 100;
                            actualElemento.PesoFinale = actualElemento.QtaDaCaricare + actualElemento.PesoIniziale;
                            actualElemento.PercFinale = actualElemento.PesoFinale * 100 / pNewTot;

                            if (elementoNull)
                            {
                                List<RffCorrezione> listCorrezioni = ea.Correzioni.ToList();
                                listCorrezioni.Add(actualElemento);
                                ea.Correzioni = listCorrezioni.ToArray();
                            }

                        }
                    }

                }


                for (int i = 0; i < ea.Correzioni.Length; i++)
                {
                    ea.Correzioni[i].PesoFinale = Convert.ToDecimal(ea.Correzioni[i].QtaDaCaricare) + Convert.ToDecimal(ea.Correzioni[i].PesoIniziale);
                    ea.Correzioni[i].PercFinale = Convert.ToDecimal(Convert.ToDecimal(ea.Correzioni[i].PesoFinale) * 100 / pNewTot);
                }

                bCalcolo = false;

            }

            return ea;

        }

        //[HttpPost]
        //[ActionName("SalvaAnalisi")]
        //public async Task<RetCode> SalvaAnalisi([FromBody] SalvaAnalisiParam param)
        //{
        //    RetCode ret = new RetCode((int)Globals.RetCodes.Ok, "");
        //    int IdMateria, idAnalisys;
        //    Correzione actRowAppoggio;

        //    ret = await DeleteChemicalAnalysis(param.ActualAnalisi.VacawIdColata, param.ActualAnalisi.VacawStato);

        //    idAnalisys = await GetNewChemicalId(param.ElencoAnalisi.IdImpianto);

        //    foreach (Correzione c in param.ElencoAnalisi.Correzioni)
        //    {
        //        try
        //        {
        //            IdMateria = (await GetElementIdByCode(c.Elemento))[0].EcId;
        //        }
        //        catch (Exception ex)
        //        {
        //            Globals.g_Logger.Error(ex, "GetElementIdByCode");
        //            continue;
        //        }

        //        try
        //        {
        //            actRowAppoggio = Globals.g_CorrezioniAppoggio.Where(co => co.Elemento == c.Elemento).FirstOrDefault();
        //        }
        //        catch (Exception ex)
        //        {
        //            Globals.g_Logger.Error(ex, "dtCorrezioniAppoggio.Rows.Find(row['ELEMENTO'])");
        //            continue;
        //        }

        //        if (param.Accettazione.Value)
        //            if (actRowAppoggio != null)
        //                ret = await InsertChemicalCorrections(idAnalisys, param.ActualAnalisi.VacawIdColata, IdMateria,
        //                                                            (float)c.PesoIniziale,
        //                                                            (float)c.PercIniziale,
        //                                                            (float)actRowAppoggio.QtaDaCaricare,
        //                                                            (float)c.QtaDaCaricare,
        //                                                            param.ActualAnalisi.VacawStato, DateTime.Now);
        //            else
        //                ret = await InsertChemicalCorrections(idAnalisys, param.ActualAnalisi.VacawIdColata, IdMateria,
        //                                                           (float)c.PesoIniziale,
        //                                                           (float)c.PercIniziale,
        //                                                           (float)0.0,
        //                                                           (float)c.QtaDaCaricare,
        //                                                           param.ActualAnalisi.VacawStato, DateTime.Now);
        //        else
        //            ret = await InsertChemicalCorrections(idAnalisys, param.ActualAnalisi.VacawIdColata, IdMateria,
        //                                                        (float)c.PesoIniziale,
        //                                                            (float)c.PercIniziale,
        //                                                            (float)actRowAppoggio.QtaDaCaricare,
        //                                                        0,
        //                                                        param.ActualAnalisi.VacawStato, DateTime.Now);
        //    }
        //    return ret;
        //}


        [HttpPost]
        [ActionName("SalvaAnalisi")]
        public async Task<RetCode> SalvaAnalisi([FromBody] SalvaAnalisiParam param)
        {
            RetCode ret = new RetCode((int)Globals.RetCodes.Ok, "");
            List<Correzioni> corr = new List<Correzioni>();
            try
            {
                bool cariche = false;

                foreach (RffCorrezione rffcorr in param.ElencoAnalisi.CorrezioniMaterie)
                {
                    VCorrezioni actualCorrezione = await _context.VCorrezioni.Where(vc => vc.VcIdMateria == rffcorr.IdMateriaCorrettivo).FirstOrDefaultAsync();
                    if (actualCorrezione != null)
                        if (actualCorrezione.VcQtaCaricata != 0)
                            cariche = true;
                }

                if (!cariche)
                {
                    Correzioni[] toRemove = await _context.Correzioni.Where(vc => vc.CIdAnalisi == param.ActualAnalisi.VacawId).ToArrayAsync();
                    _context.Correzioni.RemoveRange(toRemove);
                    await _context.SaveChangesAsync();
                }


                if (param.Accettazione.Value)
                {
                    foreach (RffCorrezione c in param.ElencoAnalisi.CorrezioniMaterie)
                    {
                        Correzioni correzione = new Correzioni();
                        correzione.CData = param.Data;
                        correzione.CIdMateria = c.IdMateriaCorrettivo.Value;
                        correzione.COperatore = param.Operatore;
                        correzione.CQtaDaCaricare = (float)c.QtaDaCaricare;
                        correzione.CIdAnalisi = param.ActualAnalisi.VacawId;
                        correzione.CStatoCarica = (int)Globals.StatiCaricheCorrezioni.ACCETTATA;
                        if (param.IsBase.Value)
                            correzione.CIdColataBase = param.ActualAnalisi.VacawIdColata;
                        else
                            correzione.CIdColata = param.ActualAnalisi.VacawIdColata;

                        corr.Add(correzione);
                    }

                    await _context.Correzioni.AddRangeAsync(corr.ToArray());
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "InsertNewChemicalAnalysis");
                ret.retCode = (int)Globals.RetCodes.NotOk;
            }

            return ret;
        }

        #region UTILITES

        public async Task<ColateLegaBase[]> GetScheduleBaseByNumber(string sIdSchedule, bool useProgNumber = false)
        {
            ColateLegaBase[] analisiBase;

            try
            {
                analisiBase = await _context.ColateLegaBase.Where(clb => clb.ClbNumeroColata == sIdSchedule && clb.ClbStato != 5).ToArrayAsync();
            }
            catch (Exception ex)
            {
                analisiBase = null;
                Globals.g_Logger.Error(ex, "GetScheduleBaseByNumber");
            }
            return analisiBase;

        }

        public async Task<ColateLegaSpecifica[]> GetScheduleSpecificaByNumber(string sIdSchedule, bool useProgNumber = false)
        {
            ColateLegaSpecifica[] analisi;

            try
            {
                if (useProgNumber)
                    analisi = await _context.ColateLegaSpecifica.Where(cls => cls.ClsNumColataProg == Convert.ToInt32(sIdSchedule) && cls.ClsStato != 5).ToArrayAsync();
                else
                    analisi = await _context.ColateLegaSpecifica.Where(cls => cls.ClsNumeroColata == sIdSchedule && cls.ClsStato != 5).ToArrayAsync();
            }
            catch (Exception ex)
            {
                analisi = null;
                Globals.g_Logger.Error(ex, "GetScheduleSpecificaByNumber");
            }
            return analisi;

        }

        public async Task<DizStatiAnalisi[]> GetPhaseByName(string sName)
        {
            DizStatiAnalisi[] phase;

            try
            {
                phase = await _context.DizStatiAnalisi.Where(ds => ds.DsaNome == sName.Trim()).ToArrayAsync();
            }
            catch (Exception ex)
            {
                phase = null;
                Globals.g_Logger.Error(ex, "GetPhaseByName");
            }
            return phase;

        }

        public async Task<ElementiChimici[]> GetElementIdByCode(string sCodice)
        {
            ElementiChimici[] elementiChimici;

            try
            {
                elementiChimici = await _context.ElementiChimici.Where(ec => ec.EcCodice == sCodice).ToArrayAsync();
            }
            catch (Exception ex)
            {
                elementiChimici = null;
                Globals.g_Logger.Error(ex, "GetElementIdByCode");
            }
            return elementiChimici;
        }

        public async Task<RetCode> InsertNewChemicalAnalysis(bool isBase, string sIdColata, string sIdMateriaPrima, string sPercIniz, string sStato, DateTime dtTimeStamp, string idAc = "", string descrizioneFase = "", string idQuantometro = "", bool perCliente = false)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");
            int id;
            try
            {
                if (!isBase)
                {
                    AnalisiChimiche ac = new AnalisiChimiche();
                    if (idAc == "")
                    {
                        if (await _context.AnalisiChimiche.CountAsync() == 0)
                            id = 0;
                        else
                            id = Convert.ToInt32(await _context.AnalisiChimiche.MaxAsync(acs => acs.AcId));
                        ac.AcId = id + 1;
                    }
                    else
                        ac.AcId = Convert.ToInt32(idAc);

                    ac.AcIdColata = Convert.ToInt32(sIdColata);
                    ac.AcIdMateriaPrima = Convert.ToInt32(sIdMateriaPrima);
                    ac.AcPercRilevata = (float)Convert.ToDouble(sPercIniz, CultureInfo.InvariantCulture);
                    ac.AcStato = Convert.ToInt32(sStato);
                    ac.AcTimestamp = dtTimeStamp;
                    ac.AcDescr = descrizioneFase;
                    ac.AcIdQuantometro = idQuantometro;
                    ac.AcPerCliente = perCliente;

                    await _context.AnalisiChimiche.AddAsync(ac);

                }
                else
                {
                    AnalisiChimicheBase acb = new AnalisiChimicheBase();
                    if (idAc == "")
                    {
                        if (await _context.AnalisiChimicheBase.CountAsync() == 0)
                            id = 0;
                        else
                            id = Convert.ToInt32(await _context.AnalisiChimicheBase.MaxAsync(acs => acs.AcbId));
                        acb.AcbId = id + 1;
                    }
                    else
                        acb.AcbId = Convert.ToInt32(idAc);

                    acb.AcbIdColata = Convert.ToInt32(sIdColata);
                    acb.AcbIdMateriaPrima = Convert.ToInt32(sIdMateriaPrima);
                    acb.AcbPercRilevata = (float)Convert.ToDouble(sPercIniz, CultureInfo.InvariantCulture);
                    acb.AcbStato = Convert.ToInt32(sStato);
                    acb.AcbTimestamp = dtTimeStamp;
                    acb.AcbDescr = descrizioneFase;
                    acb.AcbIdQuantometro = idQuantometro;
                    acb.AcbPerCliente = perCliente;

                    await _context.AnalisiChimicheBase.AddAsync(acb);
                }

                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "InsertNewChemicalAnalysis");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        public async Task<AnalisiChimicheBase> GetNewChemicalAnalysisBase(string sIdColata, string sIdMateriaPrima, string sPercIniz)
        {
            AnalisiChimicheBase acb;

            try
            {
                acb = await _context.AnalisiChimicheBase.Where(ac => ac.AcbIdColata == Convert.ToInt32(sIdColata) &&
                ac.AcbIdMateriaPrima == Convert.ToInt32(sIdMateriaPrima) && ac.AcbPercRilevata == (float)Convert.ToDouble(sPercIniz, CultureInfo.InvariantCulture)).OrderByDescending(ac => ac.AcbTimestamp).FirstOrDefaultAsync();

            }
            catch (Exception ex)
            {
                acb = null;
                Globals.g_Logger.Error(ex, "GetNewChemicalAnalysisBase");
            }
            return acb;

        }

        public async Task<AnalisiChimiche> GetNewChemicalAnalysisSpecifiche(string sIdColata, string sIdMateriaPrima, string sPercIniz)
        {
            AnalisiChimiche ac;

            try
            {
                ac = await _context.AnalisiChimiche.Where(a => a.AcIdColata == Convert.ToInt32(sIdColata) &&
                a.AcIdMateriaPrima == Convert.ToInt32(sIdMateriaPrima) && a.AcPercRilevata == (float)Convert.ToDouble(sPercIniz, CultureInfo.InvariantCulture)).OrderByDescending(a => a.AcTimestamp).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                ac = null;
                Globals.g_Logger.Error(ex, "GetNewChemicalAnalysisSpecifiche");
            }
            return ac;

        }

        private async Task<object[]> ElencoPercentuali(string sMateria, int iIdColata, int iIdImpianto)
        {
            object[] sPercentuali;
            VMaterieLegheAllWeb[] dt;

            dt = await GetPercentageByElement(sMateria, iIdColata, iIdImpianto);

            sPercentuali = new object[] { dt[0].VmlawPercNominale, dt[0].VmlawPercminN, dt[0].VmlawPercmaxN, dt[0].VmlawPercminC, dt[0].VmlawPercmaxC, dt[0].VmlawPercminR, dt[0].VmlawPercmaxR };

            return sPercentuali;
        }

        public async Task<VMaterieLegheAllWeb[]> GetPercentageByElement(string sCodice, int iIdColata, int iIdImpianto)
        {
            VMaterieLegheAllWeb[] mlaw;

            try
            {
                if (iIdImpianto == 0)
                {
                    mlaw = await _context.VMaterieLegheAllWeb.Join(_context.ElementiChimici,
                        ml => ml.VmlawIdMateria,
                        ec => ec.EcId,
                        (ml, ec) => new { VMaterieLegheAllWeb = ml, ElementiChimici = ec }).
                        Where(e => e.ElementiChimici.EcCodice == sCodice && e.VMaterieLegheAllWeb.VmlawIdColata == iIdColata && e.VMaterieLegheAllWeb.VmlawIsBase.Value).Select(m => m.VMaterieLegheAllWeb).ToArrayAsync();
                }
                else
                {
                    mlaw = await _context.VMaterieLegheAllWeb.Join(_context.ElementiChimici,
                        ml => ml.VmlawIdMateria,
                        ec => ec.EcId,
                        (ml, ec) => new { VMaterieLegheAllWeb = ml, ElementiChimici = ec }).
                        Where(e => e.ElementiChimici.EcCodice == sCodice && e.VMaterieLegheAllWeb.VmlawIdColata == iIdColata && !e.VMaterieLegheAllWeb.VmlawIsBase.Value).Select(m => m.VMaterieLegheAllWeb).ToArrayAsync();
                }
            }
            catch (Exception ex)
            {
                mlaw = null;
                Globals.g_Logger.Error(ex, "GetPercentageByElement");
            }
            return mlaw;
        }

        public async Task<VAnalisiChimicheAllWeb[]> GetChemicalByID(int iIdImpianto, int iIdAnalisi)
        {
            VAnalisiChimicheAllWeb[] acaw;

            try
            {
                acaw = await _context.VAnalisiChimicheAllWeb.Where(a => a.VacawId == iIdAnalisi && a.VacawDestinazione == iIdImpianto).OrderByDescending(a => a.VacawPercRilevata).ToArrayAsync();
            }
            catch (Exception ex)
            {
                acaw = null;
                Globals.g_Logger.Error(ex, "GetChemicalByID");
            }
            return acaw;
        }

        public decimal PesoTotaleColata(RffRilevazione[] rilevazioni)
        {
            //per ogni riga vado a ricalcolare il totale a seguto delle modifiche della riga precedente
            decimal dPesoTotColata = 0;

            foreach (RffRilevazione r in rilevazioni)
                dPesoTotColata += Convert.ToDecimal(r.Peso);

            return dPesoTotColata;
        }

        public decimal PesoTotaleColataCorretto(VCorrezioni[] correzioni)
        {
            //per ogni riga vado a ricalcolare il totale a seguto delle modifiche della riga precedente
            decimal dPesoTotColata = 0;

            foreach (VCorrezioni c in correzioni)
                dPesoTotColata += Convert.ToDecimal(c.VcQtaDaCaricare);

            return dPesoTotColata;
        }

        public async Task<RetCode> DeleteChemicalAnalysis(int iId, int iStato)
        {
            RetCode rRet = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                Globals.g_Logger.Info("Elimino le analisi chimiche");

                AnalisiChimiche[] analisi = await _context.AnalisiChimiche.Where(ac => ac.AcIdColata == iId && ac.AcStato == iStato).ToArrayAsync();
                if (analisi != null)
                {
                    _context.AnalisiChimiche.RemoveRange(analisi);
                    _ = await _context.SaveChangesAsync();
                }
                else
                    rRet = new RetCode((int)Globals.RetCodes.NoDataFound, "Analisi Non Trovata");
            }
            catch (Exception ex)
            {
                rRet.retCode = (int)Globals.RetCodes.NotOk;
                Globals.g_Logger.Error(ex, "DeleteChimicalAnalysis");
            }

            return rRet;
        }

        public async Task<int> GetNewChemicalId(int iIdImpianto)
        {
            int newId;
            try
            {
                Globals.g_Logger.Info("GENERO NUOVO ID COLATA");


                if (iIdImpianto == 0)
                {
                    if ((await _context.AnalisiChimicheBase.CountAsync()) == 0)
                        newId = 0;
                    else
                        newId = await _context.AnalisiChimicheBase.MaxAsync(acb => acb.AcbId);

                    newId++;

                }
                else
                {
                    if ((await _context.AnalisiChimiche.CountAsync()) == 0)
                        newId = 0;
                    else
                        newId = await _context.AnalisiChimiche.MaxAsync(ac => ac.AcId);

                    newId++;

                }
            }
            catch (Exception ex)
            {
                newId = 1;
                Globals.g_Logger.Error(ex, "GetNewChemicalId");
            }

            return newId;
        }

        public async Task<RetCode> InsertChemicalCorrections(int idAnalisys, int idColata, int idMateriaPrima, float sPesoIniz, float sPercIniz, float sPesoProp, float sPesoCar, int idStato, DateTime dtTimeStamp)
        {
            RetCode rRet = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                AnalisiChimiche analisi = new AnalisiChimiche();
                analisi.AcCorrezioneEffettuata = sPesoCar;
                analisi.AcCorrezioneProposta = sPesoProp;
                analisi.AcId = idAnalisys;
                analisi.AcIdColata = idColata;
                analisi.AcIdMateriaPrima = idMateriaPrima;
                analisi.AcPercRilevata = sPercIniz;
                analisi.AcStato = idStato;
                analisi.AcTimestamp = dtTimeStamp;
                analisi.AcValoreRilevato = sPesoIniz;

                await _context.AnalisiChimiche.AddAsync(analisi);
                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                rRet.retCode = (int)Globals.RetCodes.NotOk;
                Globals.g_Logger.Error(ex, "InsertChemicalCorrections");
            }

            return rRet;
        }



        #endregion

        [HttpPost]
        [ActionName("SetAnalisiPerCliente")]
        public async Task<RetCode> SetAnalisiPerCliente([FromBody] VAnalisiChimicheAllWeb param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                if (param.VacawDestinazione == 0)
                {
                    int idAnalisi = _context.VAnalisiChimicheAllWeb.Where(vacaw => vacaw.VacawPerCliente.Value && vacaw.VacawIdColata == param.VacawIdColata).Select(vacaw => vacaw.VacawId).FirstOrDefault();

                    AnalisiChimicheBase[] perCliente = await _context.AnalisiChimicheBase.Where(acb => acb.AcbId == param.VacawId).ToArrayAsync();
                    AnalisiChimicheBase[] analisi = await _context.AnalisiChimicheBase.Where(acb => acb.AcbId == idAnalisi).ToArrayAsync();

                    for (int i = 0; i < perCliente.Length; i++)
                    {
                        perCliente[i].AcbPerCliente = true;
                        perCliente[i].AcbDataPerCliente = DateTime.Now;
                    }

                    for (int i = 0; i < analisi.Length; i++)
                    {
                        analisi[i].AcbPerCliente = false;
                        analisi[i].AcbDataPerCliente = null;
                    }

                    _context.AnalisiChimicheBase.UpdateRange(perCliente);
                    _context.AnalisiChimicheBase.UpdateRange(analisi);

                }
                else
                {
                    int idAnalisi = _context.VAnalisiChimicheAllWeb.Where(vacaw => vacaw.VacawPerCliente.Value && vacaw.VacawIdColata == param.VacawIdColata).Select(vacaw => vacaw.VacawId).FirstOrDefault(); 

                    AnalisiChimiche[] perCliente = await _context.AnalisiChimiche.Where(ac => ac.AcId == param.VacawId).ToArrayAsync();
                    AnalisiChimiche[] analisi = await _context.AnalisiChimiche.Where(ac => ac.AcId == idAnalisi).ToArrayAsync();

                    for (int i=0; i< perCliente.Length; i++)
                    {
                        perCliente[i].AcPerCliente = true;
                        perCliente[i].AcDataPerCliente = DateTime.Now;
                    }

                    for (int i = 0; i < analisi.Length; i++)
                    {
                        analisi[i].AcPerCliente = false;
                        analisi[i].AcDataPerCliente = null;
                    }

                    _context.AnalisiChimiche.UpdateRange(perCliente);
                    _context.AnalisiChimiche.UpdateRange(analisi);
                }
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "SetAnalisiPerCliente");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }


        [HttpPost]
        [ActionName("GetColateSpecificheListByForno")]
        public async Task<VColataAnalisiMetallografiche[]> GetColateSpecificheListByForno([FromBody] VColataAnalisiMetallografiche param)
        {
            List<VColataAnalisiMetallografiche> colate = new List<VColataAnalisiMetallografiche>();

            colate = await _context.VColataAnalisiMetallografiche.Where(vca => 
            vca.VcamIdDestinazione == param.VcamIdDestinazione && vca.VcamAnno == param.VcamAnno).OrderByDescending(vca => vca.VcamDataInizio).ToListAsync();

            return colate.ToArray();
        }



    }
}
