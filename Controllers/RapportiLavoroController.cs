using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RapportiLavoro.Global;
using RapportiLavoro.Models;
using RapportiLavoro.Models.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RapportiLavoro.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RapportiLavoroController : Controller
    {
        private readonly RFFContext _context;

        public RapportiLavoroController(RFFContext context)
        {
            _context = context;
        }

        #region GET RAPPORTI

        [HttpPost]
        [ActionName("GetRapportoFornoFusorioActualColata")]
        public async Task<VRapportiLavoroBase> GetRapportoFornoFusorioActualColata()
        {

            ColateLegaBase actualClb;
            actualClb = await _context.ColateLegaBase.Where(clb => clb.ClbStato == 3).FirstOrDefaultAsync();

            VRapportiLavoroBase rapportoLavoroBase;
            rapportoLavoroBase = await _context.VRapportiLavoroBase.Where(rlb => rlb.VrlbId == actualClb.ClbId).FirstOrDefaultAsync();
            return rapportoLavoroBase;
        }

        [HttpPost]
        [ActionName("GetRapportoForniBacinoActualColata")]
        public async Task<VRapportiLavoroSpecifici> GetRapportoForniBacinoActualColata([FromBody] DizImpianti impianto)
        {

            ColateLegaSpecifica actualCls;
            actualCls = await _context.ColateLegaSpecifica.Where(cls => cls.ClsStato == 3 && cls.ClsDestinazione == impianto.DiCodice).FirstOrDefaultAsync();

            VRapportiLavoroSpecifici rapportoLavoroSpecifici;
            rapportoLavoroSpecifici = await _context.VRapportiLavoroSpecifici.Where(rls => rls.VrlsId == actualCls.ClsId).FirstOrDefaultAsync();
            return rapportoLavoroSpecifici;
        }

        [HttpPost]
        [ActionName("GetRapportiForniBacinoByImpianto")]
        public async Task<VRapportiLavoroSpecifici[]> GetRapportiForniBacinoByImpianto([FromBody] DizImpianti impianto)
        {

            VRapportiLavoroSpecifici[] rapportiLavoroSpecifici;
            rapportiLavoroSpecifici = await _context.VRapportiLavoroSpecifici.Where(rls => rls.VrlsIdDestinazione == impianto.DiCodice && rls.VrlsDataInizio.HasValue).OrderByDescending(rls => rls.VrlsDataInizio).ToArrayAsync();
            return rapportiLavoroSpecifici;
        }

        [HttpPost]
        [ActionName("GetRapportoColataContinuaActualColata")]
        public async Task<VRapportiLavoroSpecifici> GetRapportoColataContinuaActualColata()
        {

            ColateLegaSpecifica actualCls;
            actualCls = await _context.ColateLegaSpecifica.Where(cls => cls.ClsStato == 4).OrderByDescending(cls => cls.ClsFineOper).FirstOrDefaultAsync();

            VRapportiLavoroSpecifici rapportoLavoroSpecifici;
            rapportoLavoroSpecifici = await _context.VRapportiLavoroSpecifici.Where(rls => rls.VrlsId == actualCls.ClsId).FirstOrDefaultAsync();
            return rapportoLavoroSpecifici;
        }

        [HttpPost]
        [ActionName("GetRapportoMagazzinoPaniActualColata")]
        public async Task<VRapportiLavoroSpecifici> GetRapportoMagazzinoPaniActualColata()
        {

            ColateLegaSpecifica actualCls;
            actualCls = await _context.ColateLegaSpecifica.Where(cls => cls.ClsStato == 4).OrderByDescending(cls => cls.ClsFineOper).FirstOrDefaultAsync();

            VRapportiLavoroSpecifici rapportoLavoroSpecifici;
            rapportoLavoroSpecifici = await _context.VRapportiLavoroSpecifici.Where(rls => rls.VrlsId == actualCls.ClsId).FirstOrDefaultAsync();
            return rapportoLavoroSpecifici;
        }

        [HttpPost]
        [ActionName("GetRapportiColataContinua")]
        public async Task<VRapportiLavoroSpecifici[]> GetRapportiColataContinua()
        {

            VRapportiLavoroSpecifici[] rapportiLavoroSpecifici;
            rapportiLavoroSpecifici = await _context.VRapportiLavoroSpecifici.Where(rls => rls.VrlsDurata.HasValue).OrderByDescending(rls => rls.VrlsDataFine).ToArrayAsync();
            return rapportiLavoroSpecifici;
        }

        [HttpPost]
        [ActionName("GetRapportiMagazzinoPani")]
        public async Task<VRapportiLavoroSpecifici[]> GetRapportiMagazzinoPani()
        {

            VRapportiLavoroSpecifici[] rapportiLavoroSpecifici;
            rapportiLavoroSpecifici = await _context.VRapportiLavoroSpecifici.Where(rls => rls.VrlsDurata.HasValue).OrderByDescending(rls => rls.VrlsDataFine).ToArrayAsync();
            return rapportiLavoroSpecifici;
        }


        #endregion

        #region FERMI

        [HttpPost]
        [ActionName("GetFermiFornoFusorio")]
        public async Task<VListaFermi[]> GetFermiFornoFusorio([FromBody] TurnoOperatori shift)
        {
            List<VListaFermi> fermi = new List<VListaFermi>();
            fermi = await _context.VListaFermi.Where(a => (!a.VlfIdFermo.HasValue ||
            (a.VlfDataInizio >= shift.ToStartDate.Value.ToUniversalTime() && a.VlfDataFine <= shift.ToEndDate.Value.ToUniversalTime()))
            && a.VlfIdImpianto == (int)Global.Globals.IdImpianto.FORNO_FUSORIO).OrderByDescending(a => a.VlfDataInizio).ToListAsync();
            return fermi.ToArray();
        }

        [HttpPost]
        [ActionName("GetFermiColataContinuaByColata")]
        public async Task<VListaFermi[]> GetFermiColataContinuaByColata([FromBody] VColateSpecifiche colata)
        {
            List<VListaFermi> fermi = new List<VListaFermi>();
            fermi = await _context.VListaFermi.Where(a => a.VlfIdColataSpecifica == colata.VcsId && a.VlfIdImpianto == (int)Global.Globals.IdImpianto.COLATA_CONTINUA).OrderByDescending(a => a.VlfDataInizio).ToListAsync();
            return fermi.ToArray();
        }

        [HttpPost]
        [ActionName("GetFermiMagazzinoPaniByColata")]
        public async Task<VListaFermi[]> GetFermiMagazzinoPaniByColata([FromBody] VColateSpecifiche colata)
        {
            List<VListaFermi> fermi = new List<VListaFermi>();
            fermi = await _context.VListaFermi.Where(a => a.VlfIdColataSpecifica == colata.VcsId && a.VlfIdImpianto == (int)Global.Globals.IdImpianto.MAGAZZINO_PANI).OrderByDescending(a => a.VlfDataInizio).ToListAsync();
            return fermi.ToArray();
        }

        [HttpPost]
        [ActionName("GetFermiForniBacino")]
        public async Task<VListaFermi[]> GetFermiForniBacino([FromBody] TurnoOperatori shift)
        {
            List<VListaFermi> fermi = new List<VListaFermi>();
            fermi = await _context.VListaFermi.Where(a => (!a.VlfIdFermo.HasValue ||
            (a.VlfDataInizio >= shift.ToStartDate.Value.ToUniversalTime() && a.VlfDataFine <= shift.ToEndDate.Value.ToUniversalTime()))
            && a.VlfIdImpianto == (int)Global.Globals.IdImpianto.FORNI_BACINO).OrderByDescending(a => a.VlfDataInizio).ToListAsync();
            return fermi.ToArray();
        }

        [HttpPost]
        [ActionName("AddFermo")]
        public async Task<RetCode> AddFermo([FromBody] Fermi fb)
        {
            RetCode rc = new RetCode((int)Global.Globals.RetCodes.Ok, "");

            try
            {
                await _context.Fermi.AddAsync(fb);
                int i = await _context.SaveChangesAsync();

                rc.retCode = fb.FId;
            }
            catch (Exception ex)
            {
                rc.retCode = (int)Global.Globals.RetCodes.NotOk;
                rc.note = ex.Message;
            }
            return rc;
        }

        [HttpPost]
        [ActionName("AddListaFermo")]
        public async Task<RetCode> AddListaFermo([FromBody] ListaFermi lf)
        {
            RetCode rc = new RetCode((int)Global.Globals.RetCodes.Ok, "");

            try
            {
                await _context.ListaFermi.AddAsync(lf);
                int i = await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                rc.retCode = (int)Global.Globals.RetCodes.NotOk;
                rc.note = ex.Message;
            }
            return rc;
        }

        [HttpPost]
        [ActionName("EditListaFermo")]
        public async Task<RetCode> EditListaFermo([FromBody] ListaFermi lf)
        {
            RetCode rc = new RetCode((int)Global.Globals.RetCodes.Ok, "");

            try
            {
                _context.ListaFermi.Update(lf);
                _context.SaveChanges();

            }
            catch (Exception ex)
            {
                rc.retCode = (int)Global.Globals.RetCodes.NotOk;
                rc.note = ex.Message;
            }
            return rc;
        }

        [HttpPost]
        [ActionName("GetFirstWhyByImpianto")]
        public async Task<DizFermiA[]> GetFirstWhyByImpianto([FromBody] int idImpianto)
        {

            List<DizFermiA> primoWhy = new List<DizFermiA>();
            primoWhy = await _context.DizFermiA.Where(pw => pw.DfaImpianto == idImpianto).OrderBy(pw => pw.DfaDescrizione).ToListAsync();
            return primoWhy.ToArray();
        }

        [HttpPost]
        [ActionName("GetSecondWhyByImpianto")]
        public async Task<DizFermiB[]> GetSecondWhyByImpianto([FromBody] int idImpianto)
        {
            List<DizFermiB> secondoWhy = new List<DizFermiB>();
            secondoWhy = await _context.DizFermiB.Where(sw => sw.DfbImpianto == idImpianto).OrderBy(sw => sw.DfbDescrizione).ToListAsync();
            return secondoWhy.ToArray();
        }

        [HttpPost]
        [ActionName("GetThirdWhyByImpianto")]
        public async Task<DizFermiC[]> GetThirdWhyByImpianto([FromBody] int idImpianto)
        {
            List<DizFermiC> terzoWhy = new List<DizFermiC>();
            terzoWhy = await _context.DizFermiC.Where(tw => tw.DfcImpianto == idImpianto).OrderBy(tw => tw.DfcDescrizione).ToListAsync();
            return terzoWhy.ToArray();
        }

        [HttpPost]
        [ActionName("GetFourthWhyByImpianto")]
        public async Task<DizFermiD[]> GetFourthWhyByImpianto([FromBody] int idImpianto)
        {
            List<DizFermiD> quartoWhy = new List<DizFermiD>();
            quartoWhy = await _context.DizFermiD.Where(qw => qw.DfdImpianto == idImpianto).OrderBy(qw => qw.DfdDescrizione).ToListAsync();
            return quartoWhy.ToArray();
        }

        [HttpPost]
        [ActionName("GetFifthWhyByImpianto")]
        public async Task<DizFermiE[]> GetFifthWhyByImpianto([FromBody] int idImpianto)
        {
            List<DizFermiE> quitnoWhy = new List<DizFermiE>();
            quitnoWhy = await _context.DizFermiE.Where(qw => qw.DfeImpianto == idImpianto).OrderBy(qw => qw.DfeDescrizione).ToListAsync();
            return quitnoWhy.ToArray();
        }

        [HttpPost]
        [ActionName("GetActionByImpianto")]
        public async Task<DizFermiF[]> GetActionByImpianto([FromBody] int idImpianto)
        {
            List<DizFermiF> action = new List<DizFermiF>();
            action = await _context.DizFermiF.Where(a => a.DffImpianto == idImpianto).OrderBy(a => a.DffDescrizione).ToListAsync();
            return action.ToArray();
        }

        [HttpPost]
        [ActionName("GetSecondWhy")]
        public async Task<DizFermiB[]> GetSecondWhy([FromBody] Fermi why)
        {
            List<int> mappa = new List<int>();
            mappa = await _context.DizMappaFermi.Where(dmf => dmf.DmfColonnaA == why.FColonnaA).Select(dmf => dmf.DmfColonnaB).ToListAsync();

            List<DizFermiB> secondoWhy = new List<DizFermiB>();
            secondoWhy = await _context.DizFermiB.Where(sw => mappa.Contains(sw.DfbId)).OrderBy(sw => sw.DfbDescrizione).ToListAsync();
            return secondoWhy.ToArray();
        }

        [HttpPost]
        [ActionName("GetThirdWhy")]
        public async Task<DizFermiC[]> GetThirdWhy([FromBody] Fermi why)
        {
            List<int> mappa = new List<int>();
            mappa = await _context.DizMappaFermi.Where(dmf => dmf.DmfColonnaA == why.FColonnaA && dmf.DmfColonnaB == why.FColonnaB).
                Select(dmf => dmf.DmfColonnaC).ToListAsync();

            List<DizFermiC> terzoWhy = new List<DizFermiC>();
            terzoWhy = await _context.DizFermiC.Where(tw => mappa.Contains(tw.DfcId)).OrderBy(tw => tw.DfcDescrizione).ToListAsync();
            return terzoWhy.ToArray();
        }

        [HttpPost]
        [ActionName("GetFourthWhy")]
        public async Task<DizFermiD[]> GetFourthWhy([FromBody] Fermi why)
        {
            List<int> mappa = new List<int>();
            mappa = await _context.DizMappaFermi.Where(dmf => dmf.DmfColonnaA == why.FColonnaA && dmf.DmfColonnaB == why.FColonnaB
            && dmf.DmfColonnaC == why.FColonnaC).Select(dmf => dmf.DmfColonnaD).ToListAsync();

            List<DizFermiD> quartoWhy = new List<DizFermiD>();
            quartoWhy = await _context.DizFermiD.Where(qw => mappa.Contains(qw.DfdId)).OrderBy(qw => qw.DfdDescrizione).ToListAsync();
            return quartoWhy.ToArray();
        }

        [HttpPost]
        [ActionName("GetFifthWhy")]
        public async Task<DizFermiE[]> GetFifthWhy([FromBody] Fermi why)
        {
            List<int> mappa = new List<int>();
            mappa = await _context.DizMappaFermi.Where(dmf => dmf.DmfColonnaA == why.FColonnaA && dmf.DmfColonnaB == why.FColonnaB
            && dmf.DmfColonnaC == why.FColonnaC && dmf.DmfColonnaD == why.FColonnaD).Select(dmf => dmf.DmfColonnaE).ToListAsync();

            List<DizFermiE> quitnoWhy = new List<DizFermiE>();
            quitnoWhy = await _context.DizFermiE.Where(qw => mappa.Contains(qw.DfeId)).OrderBy(qw => qw.DfeDescrizione).ToListAsync();
            return quitnoWhy.ToArray();
        }

        [HttpPost]
        [ActionName("GetAction")]
        public async Task<DizFermiF[]> GetAction([FromBody] Fermi why)
        {
            List<int> mappa = new List<int>();
            mappa = await _context.DizMappaFermi.Where(dmf => dmf.DmfColonnaA == why.FColonnaA && dmf.DmfColonnaB == why.FColonnaB
            && dmf.DmfColonnaC == why.FColonnaC && dmf.DmfColonnaD == why.FColonnaD && dmf.DmfColonnaE == why.FColonnaE).
            Select(dmf => dmf.DmfColonnaF).ToListAsync();

            List<DizFermiF> action = new List<DizFermiF>();
            action = await _context.DizFermiF.Where(a => mappa.Contains(a.DffId)).OrderBy(a => a.DffDescrizione).ToListAsync();
            return action.ToArray();
        }

        #endregion

        #region ANOMALIE

        [HttpPost]
        [ActionName("AddAnomalia")]
        public async Task<RetCode> AddAnomalia([FromBody] Anomalie a)
        {
            RetCode rc = new RetCode((int)Global.Globals.RetCodes.Ok, "");

            try
            {
                await _context.Anomalie.AddAsync(a);
                int i = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                rc.retCode = (int)Global.Globals.RetCodes.NotOk;
                rc.note = ex.Message;
            }
            return rc;
        }

        [HttpPost]
        [ActionName("GetAnomalieFornoFusorioByShift")]
        public async Task<VAnomalie[]> GetAnomalieFornoFusorioByShift([FromBody] TurnoOperatori shift)
        {
            List<VAnomalie> anomalie = new List<VAnomalie>();
            anomalie = await _context.VAnomalie.Where(a => a.VanIdImpianto == (int)Global.Globals.IdImpianto.FORNO_FUSORIO &&
            a.VanData >= shift.ToStartDate.Value.ToUniversalTime() && a.VanData <= shift.ToEndDate.Value.ToUniversalTime()).ToListAsync();
            return anomalie.ToArray();
        }

        [HttpPost]
        [ActionName("GetAnomalieColataContinuaByColata")]
        public async Task<VAnomalie[]> GetAnomalieColataContinuaByColata([FromBody] VColateSpecifiche colata)
        {
            List<VAnomalie> anomalie = new List<VAnomalie>();
            anomalie = await _context.VAnomalie.Where(a => a.VanIdImpianto == (int)Global.Globals.IdImpianto.COLATA_CONTINUA &&
            a.VanIdColataSpecifica == colata.VcsId).ToListAsync();
            return anomalie.ToArray();
        }

        [HttpPost]
        [ActionName("GetAnomalieMagazzinoPaniByColata")]
        public async Task<VAnomalie[]> GetAnomalieMagazzinoPaniByColata([FromBody] VColateSpecifiche colata)
        {
            List<VAnomalie> anomalie = new List<VAnomalie>();
            anomalie = await _context.VAnomalie.Where(a => a.VanIdImpianto == (int)Global.Globals.IdImpianto.MAGAZZINO_PANI &&
            a.VanIdColataSpecifica == colata.VcsId).ToListAsync();
            return anomalie.ToArray();
        }

        [HttpPost]
        [ActionName("GetAnomalieForniBacinoByColata")]
        public async Task<VAnomalie[]> GetAnomalieForniBacinoByColata([FromBody] VColateSpecifiche colata)
        {
            List<VAnomalie> anomalie = new List<VAnomalie>();
            anomalie = await _context.VAnomalie.Where(a => a.VanIdImpianto == (int)Global.Globals.IdImpianto.FORNI_BACINO &&
            a.VanIdColataSpecifica == colata.VcsId).ToListAsync();
            return anomalie.ToArray();
        }


        #endregion

        #region RALLENTAMENTI

        [HttpPost]
        [ActionName("AddRallentamento")]
        public async Task<RetCode> AddRallentamento([FromBody] Rallentamenti r)
        {
            RetCode rc = new RetCode((int)Global.Globals.RetCodes.Ok, "");

            try
            {
                await _context.Rallentamenti.AddAsync(r);
                int i = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                rc.retCode = (int)Global.Globals.RetCodes.NotOk;
                rc.note = ex.Message;
            }
            return rc;
        }

        [HttpPost]
        [ActionName("GetRallentamentiColataContinua")]
        public async Task<VRallentamenti[]> GetRallentamentiColataContinua([FromBody] VColateSpecifiche colata)
        {
            List<VRallentamenti> rallentamenti = new List<VRallentamenti>();
            rallentamenti = await _context.VRallentamenti.Where(vr => vr.VrIdColata == colata.VcsId).ToListAsync();
            return rallentamenti.ToArray();
        }

        #endregion

        #region NOTE

        [HttpPost]
        [ActionName("GetNoteFornoFusorioByShift")]
        public async Task<VNoteRapportiLavoro[]> GetNoteFornoFusorioByShift([FromBody] TurnoOperatori shift)
        {
            List<VNoteRapportiLavoro> note = new List<VNoteRapportiLavoro>();
            note = await _context.VNoteRapportiLavoro.Where(a => a.VnrlIdImpianto == (int)Global.Globals.IdImpianto.FORNO_FUSORIO &&
            a.VnrlData >= shift.ToStartDate.Value.ToUniversalTime() && a.VnrlData <= shift.ToEndDate.Value.ToUniversalTime()).ToListAsync();
            return note.ToArray();
        }

        [HttpPost]
        [ActionName("GetNoteColataContinuaActualColata")]
        public async Task<VNoteRapportiLavoro[]> GetNoteColataContinuaActualColata([FromBody] VColateSpecifiche colata)
        {
            List<VNoteRapportiLavoro> note = new List<VNoteRapportiLavoro>();
            note = await _context.VNoteRapportiLavoro.Where(a => a.VnrlIdImpianto == (int)Global.Globals.IdImpianto.COLATA_CONTINUA &&
                                                            a.VnrlIdColataSpecifica == colata.VcsId).ToListAsync();
            return note.ToArray();
        }

        [HttpPost]
        [ActionName("GetNoteColataContinuaPrevTenColate")]
        public async Task<VNoteRapportiLavoro[]> GetNoteColataContinuaPrevTenColate([FromBody] VColateSpecifiche colata)
        {
            List<VNoteRapportiLavoro> note = new List<VNoteRapportiLavoro>();
            ColateLegaSpecifica actualColata = await _context.ColateLegaSpecifica.Where(cls => cls.ClsId == colata.VcsId).FirstOrDefaultAsync();

            List<ColateLegaSpecifica> lastTenColate = new List<ColateLegaSpecifica>();
            lastTenColate = await _context.ColateLegaSpecifica.Where(cls => cls.ClsFineOper < actualColata.ClsFineOper).OrderByDescending(cls => cls.ClsFineOper).Take(10).ToListAsync();

            foreach (ColateLegaSpecifica cls in lastTenColate)
            {
                List<VNoteRapportiLavoro> noteTmp = new List<VNoteRapportiLavoro>();
                noteTmp = await _context.VNoteRapportiLavoro.Where(a => a.VnrlIdImpianto == (int)Global.Globals.IdImpianto.COLATA_CONTINUA &&
                                                                a.VnrlIdColataSpecifica == cls.ClsId).ToListAsync();

                note.AddRange(noteTmp);
            }

            return note.ToArray();
        }

        [HttpPost]
        [ActionName("GetNoteMagazzinoPaniActualColata")]
        public async Task<VNoteRapportiLavoro[]> GetNoteMagazzinoPaniActualColata([FromBody] VColateSpecifiche colata)
        {
            List<VNoteRapportiLavoro> note = new List<VNoteRapportiLavoro>();
            note = await _context.VNoteRapportiLavoro.Where(a => a.VnrlIdImpianto == (int)Global.Globals.IdImpianto.MAGAZZINO_PANI &&
                                                            a.VnrlIdColataSpecifica == colata.VcsId).ToListAsync();
            return note.ToArray();
        }

        [HttpPost]
        [ActionName("GetNoteMagazzinoPaniPrevTenColate")]
        public async Task<VNoteRapportiLavoro[]> GetNoteMagazzinoPaniPrevTenColate([FromBody] VColateSpecifiche colata)
        {
            List<VNoteRapportiLavoro> note = new List<VNoteRapportiLavoro>();
            ColateLegaSpecifica actualColata = await _context.ColateLegaSpecifica.Where(cls => cls.ClsId == colata.VcsId).FirstOrDefaultAsync();

            List<ColateLegaSpecifica> lastTenColate = new List<ColateLegaSpecifica>();
            lastTenColate = await _context.ColateLegaSpecifica.Where(cls => cls.ClsFineOper < actualColata.ClsFineOper).OrderByDescending(cls => cls.ClsFineOper).Take(10).ToListAsync();

            foreach (ColateLegaSpecifica cls in lastTenColate)
            {
                List<VNoteRapportiLavoro> noteTmp = new List<VNoteRapportiLavoro>();
                noteTmp = await _context.VNoteRapportiLavoro.Where(a => a.VnrlIdImpianto == (int)Global.Globals.IdImpianto.MAGAZZINO_PANI &&
                                                                a.VnrlIdColataSpecifica == cls.ClsId).ToListAsync();

                note.AddRange(noteTmp);
            }

            return note.ToArray();
        }

        [HttpPost]
        [ActionName("GetNoteForniBacinoByShift")]
        public async Task<VNoteRapportiLavoro[]> GetNoteForniBacinoByShift([FromBody] TurnoOperatori shift)
        {
            List<VNoteRapportiLavoro> note = new List<VNoteRapportiLavoro>();
            note = await _context.VNoteRapportiLavoro.Where(a => a.VnrlIdImpianto == (int)Global.Globals.IdImpianto.FORNI_BACINO &&
            a.VnrlData >= shift.ToStartDate.Value.ToUniversalTime() && a.VnrlData <= shift.ToEndDate.Value.ToUniversalTime()).ToListAsync();
            return note.ToArray();
        }

        [HttpPost]
        [ActionName("AddNota")]
        public async Task<RetCode> AddNota([FromBody] NoteRapportiLavoro nb)
        {
            RetCode rc = new RetCode((int)Global.Globals.RetCodes.Ok, "");

            try
            {
                await _context.NoteRapportiLavoro.AddAsync(nb);
                int i = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                rc.retCode = (int)Global.Globals.RetCodes.NotOk;
                rc.note = ex.Message;
            }
            return rc;
        }

        #endregion

        #region PASSAGGIO DI CONSEGNE

        [HttpPost]
        [ActionName("GetPassaggioConsegneColataContinua")]
        public async Task<VPassaggioConsegne[]> GetPassaggioConsegneColataContinua([FromBody] VColateSpecifiche colata)
        {
            List<VPassaggioConsegne> passaggioConsegne = new List<VPassaggioConsegne>();
            passaggioConsegne = await _context.VPassaggioConsegne.Where(pc => pc.VpcIdImpianto == (int)Global.Globals.IdImpianto.COLATA_CONTINUA &&
                                                            pc.VpcIdColataSpecifica == colata.VcsId).ToListAsync();
            return passaggioConsegne.ToArray();
        }

        [HttpPost]
        [ActionName("GetPassaggioConsegneMagazzinoPani")]
        public async Task<VPassaggioConsegne[]> GetPassaggioConsegneMagazzinoPani([FromBody] VColateSpecifiche colata)
        {
            List<VPassaggioConsegne> passaggioConsegne = new List<VPassaggioConsegne>();
            passaggioConsegne = await _context.VPassaggioConsegne.Where(pc => pc.VpcIdImpianto == (int)Global.Globals.IdImpianto.MAGAZZINO_PANI &&
                                                            pc.VpcIdColataSpecifica == colata.VcsId).ToListAsync();
            return passaggioConsegne.ToArray();
        }


        #endregion

        #region CARICHI

        [HttpPost]
        [ActionName("GetMaterieSchedulateByColata")]
        public async Task<VMaterieLegheAllWeb[]> GetMaterieSchedulateByColata([FromBody] VColateAll colata)
        {

            List<VMaterieLegheAllWeb> action = new List<VMaterieLegheAllWeb>();
            action = await _context.VMaterieLegheAllWeb.Where(vmlaw => vmlaw.VmlawIdColata == colata.VcaId && vmlaw.VmlawTipoMateriale != 4).OrderByDescending(vmlaw => vmlaw.VmlawQta).ToListAsync();
            return action.ToArray();
        }

        [HttpPost]
        [ActionName("GetCorrezioniByColata")]
        public async Task<VCorrezioni[]> GetCorrezioniByColata([FromBody] VColateAll colata)
        {

            List<VCorrezioni> action = new List<VCorrezioni>();
            action = await _context.VCorrezioni.Where(correzione => correzione.VcIdColataSpecifica == colata.VcaId).ToListAsync();
            return action.ToArray();
        }

        [HttpPost]
        [ActionName("GetCarichiMateriale")]
        public async Task<VCarichiLegheDetail[]> GetCarichiMateriale([FromBody] Filter f)
        {

            List<VCarichiLegheDetail> action = new List<VCarichiLegheDetail>();
            if (f.StartDate != f.EndDate)//date impostate uguali se non da filtrare
                action = await _context.VCarichiLegheDetail.Where(vcld => vcld.VmpsdIdLega == f.IdColata && vcld.VmpsdIdMateria == f.IdMateria &&
                    vcld.VmpsdTxnDate >= f.StartDate && vcld.VmpsdTxnDate <= f.EndDate).ToListAsync();
            else
                action = await _context.VCarichiLegheDetail.Where(vcld => vcld.VmpsdIdCorrezione == f.IdCorrezione/*vcld.VmpsdIdLega == f.IdColata && vcld.VmpsdIdMateria == f.IdMateria*/).ToListAsync();
            return action.ToArray();
        }

        [HttpPost]
        [ActionName("AddCaricoBase")]
        public async Task<RetCode> AddCaricoBase([FromBody] CarichiLegheBase carico)
        {

            RetCode rc = new RetCode((int)Global.Globals.RetCodes.Ok, "");

            try
            {
                await _context.CarichiLegheBase.AddAsync(carico);
                int i = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                rc.retCode = (int)Global.Globals.RetCodes.NotOk;
                rc.note = ex.Message;
            }
            return rc;
        }

        [HttpPost]
        [ActionName("AddCaricoSpecifico")]
        public async Task<RetCode> AddCaricoSpecifico([FromBody] CarichiLegheSpecifiche carico)
        {

            RetCode rc = new RetCode((int)Global.Globals.RetCodes.Ok, "");

            try
            {
                Correzioni correzioneTmp = await _context.Correzioni.Where(c => c.CId == carico.CalsIdCorrezione).FirstOrDefaultAsync();

                if (correzioneTmp != null)
                    if (correzioneTmp.CStatoCarica == (int)Globals.StatiCaricheCorrezioni.ACCETTATA || correzioneTmp.CStatoCarica == (int)Globals.StatiCaricheCorrezioni.NON_PIANIFICATA)
                    {
                        correzioneTmp.CStatoCarica = (int)Globals.StatiCaricheCorrezioni.IN_CORSO;
                        _context.Correzioni.Update(correzioneTmp);
                    }


                await _context.CarichiLegheSpecifiche.AddAsync(carico);
                int i = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                rc.retCode = (int)Global.Globals.RetCodes.NotOk;
                rc.note = ex.Message;
            }
            return rc;
        }

        [HttpPost]
        [ActionName("UpdateCarico")]
        public async Task<RetCode> UpdateCarico([FromBody] VCarichiLegheDetail carico)
        {

            RetCode rc = new RetCode((int)Global.Globals.RetCodes.Ok, "");

            try
            {
                if (carico.VmpsdLegaBase.Value)
                {
                    CarichiLegheBase c = await _context.CarichiLegheBase.Where(clb => clb.CalbId == carico.VmpsdId).FirstOrDefaultAsync();
                    c.CalbPercOrganico = carico.VmpsdPercOrganico;

                    _context.CarichiLegheBase.Update(c);
                    _context.SaveChanges();
                }
                else
                {
                    CarichiLegheSpecifiche c = await _context.CarichiLegheSpecifiche.Where(cls => cls.CalsId == carico.VmpsdId).FirstOrDefaultAsync();
                    c.CalsPercOrganico = carico.VmpsdPercOrganico;

                    _context.CarichiLegheSpecifiche.Update(c);
                    _context.SaveChanges();

                }
            }
            catch (Exception ex)
            {
                rc.retCode = (int)Global.Globals.RetCodes.NotOk;
                rc.note = ex.Message;
            }
            return rc;
        }

        [HttpPost]
        [ActionName("UpdateCaricoRange")]
        public async Task<RetCode> UpdateCaricoRange([FromBody] VCarichiLegheDetail[] carichi)
        {

            RetCode rc = new RetCode((int)Global.Globals.RetCodes.Ok, "");

            try
            {
                foreach (VCarichiLegheDetail carico in carichi)
                {
                    if (carico.VmpsdLegaBase.Value)
                    {
                        CarichiLegheBase c = await _context.CarichiLegheBase.Where(clb => clb.CalbId == carico.VmpsdId).FirstOrDefaultAsync();
                        c.CalbPercOrganico = carico.VmpsdPercOrganico;

                        _context.CarichiLegheBase.Update(c);

                    }
                    else
                    {
                        CarichiLegheSpecifiche c = await _context.CarichiLegheSpecifiche.Where(cls => cls.CalsId == carico.VmpsdId).FirstOrDefaultAsync();
                        c.CalsPercOrganico = carico.VmpsdPercOrganico;

                        _context.CarichiLegheSpecifiche.Update(c);

                    }
                }

                _context.SaveChanges();

            }
            catch (Exception ex)
            {
                rc.retCode = (int)Global.Globals.RetCodes.NotOk;
                rc.note = ex.Message;
            }
            return rc;
        }

        [HttpPost]
        [ActionName("DeleteCarico")]
        public async Task<RetCode> DeleteCarico([FromBody] VCarichiLegheDetail carico)
        {

            RetCode rc = new RetCode((int)Global.Globals.RetCodes.Ok, "");

            try
            {
                if (carico.VmpsdLegaBase.Value)
                {
                    CarichiLegheBase c = await _context.CarichiLegheBase.Where(clb => clb.CalbId == carico.VmpsdId).FirstOrDefaultAsync();

                    _context.CarichiLegheBase.Remove(c);
                    _context.SaveChanges();
                }
                else
                {

                    CarichiLegheSpecifiche c = await _context.CarichiLegheSpecifiche.Where(cls => cls.CalsId == carico.VmpsdId).FirstOrDefaultAsync();

                    _context.CarichiLegheSpecifiche.Remove(c);
                    await _context.SaveChangesAsync();

                    VCorrezioni correzioneTmp = await _context.VCorrezioni.Where(c => c.VcId == carico.VmpsdIdCorrezione).FirstOrDefaultAsync();

                    if (correzioneTmp != null)
                        if (correzioneTmp.VcQtaCaricata == 0)
                        {
                            Correzioni correzioneUpdate = await _context.Correzioni.Where(c => c.CId == carico.VmpsdIdCorrezione).FirstOrDefaultAsync();

                            if (correzioneUpdate.CStatoCarica != (int)Globals.StatiCaricheCorrezioni.CHIUSA)
                            {
                                if (correzioneUpdate.CQtaDaCaricare == 0)
                                    correzioneUpdate.CStatoCarica = (int)Globals.StatiCaricheCorrezioni.NON_PIANIFICATA;
                                else
                                    correzioneUpdate.CStatoCarica = (int)Globals.StatiCaricheCorrezioni.ACCETTATA;
                            }

                            _context.Correzioni.Update(correzioneUpdate);
                            await _context.SaveChangesAsync();
                        }

                }
            }
            catch (Exception ex)
            {
                rc.retCode = (int)Global.Globals.RetCodes.NotOk;
                rc.note = ex.Message;
            }
            return rc;
        }

        [HttpPost]
        [ActionName("CloseCarica")]
        public async Task<RetCode> CloseCarica([FromBody] VCorrezioni param)
        {

            RetCode rc = new RetCode((int)Global.Globals.RetCodes.Ok, "");

            try
            {
                Correzioni correzione = await _context.Correzioni.Where(c => c.CId == param.VcId).FirstOrDefaultAsync();

                correzione.CStatoCarica = (int)Globals.StatiCaricheCorrezioni.CHIUSA;

                _context.Correzioni.Update(correzione);
                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                rc.retCode = (int)Global.Globals.RetCodes.NotOk;
                rc.note = ex.Message;
            }
            return rc;
        }

        [HttpPost]
        [ActionName("AddCorrezioneNonPianificata")]
        public async Task<RetCode> AddCorrezioneNonPianificata([FromBody] Correzioni c)
        {
            RetCode rc = new RetCode((int)Global.Globals.RetCodes.Ok, "");

            try
            {
                await _context.Correzioni.AddAsync(c);
                int i = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                rc.retCode = (int)Global.Globals.RetCodes.NotOk;
                rc.note = ex.Message;
            }
            return rc;
        }

        [HttpPost]
        [ActionName("AddMaterialeNonPianificato")]
        public async Task<RetCode> AddMaterialeNonPianificato([FromBody] MaterieLegheBaseSchedulate m)
        {
            RetCode rc = new RetCode((int)Global.Globals.RetCodes.Ok, "");

            try
            {
                MaterieLegheBaseSchedulate mlbs = new MaterieLegheBaseSchedulate();
                MateriePrime mp = await _context.MateriePrime.Where( materiePrime => materiePrime.MpId == m.MlbsMpId).FirstOrDefaultAsync();
                mlbs.MlbsClbId = m.MlbsClbId;
                mlbs.MlbsDestinazione = 0;
                mlbs.MlbsEcId = await _context.ElementiChimici.Where(ec => ec.EcCodice == "EL. CHIMICO").Select(ec=>ec.EcId).FirstOrDefaultAsync();
                mlbs.MlbsFormato = mp.MpFormato;
                mlbs.MlbsImpurita = mp.MpImpurita;
                mlbs.MlbsLmId = await _context.ColateLegaBase.Where(clb => clb.ClbId == m.MlbsClbId).Select(clb=>clb.ClbIdLega).FirstOrDefaultAsync();
                mlbs.MlbsMpId = m.MlbsMpId;
                mlbs.MlbsOrganico = mp.MpOrganico;
                mlbs.MlbsPercmaxC = 0;
                mlbs.MlbsPercmaxN = 0;
                mlbs.MlbsPercmaxR = 0;
                mlbs.MlbsPercminC = 0;
                mlbs.MlbsPercminN = 0;
                mlbs.MlbsPercminR = 0;
                mlbs.MlbsPercNominale = 0;
                mlbs.MlbsPercResaScoriaObiettivo = mp.MpPercResaScoriaObiettivo;
                mlbs.MlbsPercScoriaObiettivo = mp.MpPercScoriaObiettivo;
                mlbs.MlbsPezzatura = mp.MpPezzatura;
                mlbs.MlbsPrioritaInserimento = 0;
                mlbs.MlbsQtaCaricare = 0;
                mlbs.MlbsQtaCaricata = 0;
                mlbs.MlbsQtaMax = 0;
                mlbs.MlbsQtaMin = 0;
                mlbs.MlbsVersione = 1;

                await _context.MaterieLegheBaseSchedulate.AddAsync(mlbs);
                int i = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                rc.retCode = (int)Global.Globals.RetCodes.NotOk;
                rc.note = ex.Message;
            }
            return rc;
        }

        #endregion

        #region TEMPERATURE

        [HttpPost]
        [ActionName("GetLastTemperaturaBacino")]
        public async Task<TemperatureBacino> GetLastTemperaturaBacino([FromBody] VRapportiLavoroSpecifici colata)
        {

            TemperatureBacino lastTemp = await _context.TemperatureBacino.Where(tb => tb.TbIdColata == colata.VrlsId).OrderByDescending(tb => tb.TbData).FirstOrDefaultAsync();

            return lastTemp;
        }

        [HttpPost]
        [ActionName("AddTemperaturaBacino")]
        public async Task<RetCode> AddTemperaturaBacino([FromBody] TemperatureBacino param)
        {
            RetCode rc = new RetCode((int)Global.Globals.RetCodes.Ok, "");

            try
            {
                await _context.TemperatureBacino.AddAsync(param);
                int i = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                rc.retCode = (int)Global.Globals.RetCodes.NotOk;
                rc.note = ex.Message;
            }
            return rc;
        }

        #endregion

        #region PANI DA RIFONDERE

        [HttpPost]
        [ActionName("GetPaniDaRifondereListByColata")]
        public async Task<VPaniDaRifondere[]> GetPaniDaRifondereListByColata([FromBody] VRapportiLavoroSpecifici colata)
        {

            List<VPaniDaRifondere> paniDaRifondere = new List<VPaniDaRifondere>();
            paniDaRifondere = await _context.VPaniDaRifondere.Where(pdr => pdr.VpdrIdColata == colata.VrlsId).ToListAsync();

            return paniDaRifondere.ToArray();
        }

        [HttpPost]
        [ActionName("AddPaniDaRifondere")]
        public async Task<RetCode> AddPaniDaRifondere([FromBody] PaniDaRifondere param)
        {
            RetCode rc = new RetCode((int)Global.Globals.RetCodes.Ok, "");

            try
            {
                await _context.PaniDaRifondere.AddAsync(param);
                int i = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                rc.retCode = (int)Global.Globals.RetCodes.NotOk;
                rc.note = ex.Message;
            }
            return rc;
        }

        #endregion


    }
}
