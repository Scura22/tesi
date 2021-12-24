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
    public class DictionaryController : Controller
    {
        private readonly RFFContext _context;

        public DictionaryController(RFFContext context)
        {
            _context = context;
        }

        #region DIZIONARIO OUTPUT COLATA

        [HttpPost]
        [ActionName("GetDizOutputColataFornoFusorio")]
        public async Task<VDizCassoniOutput[]> GetDizOutputColataFornoFusorio([FromBody] int cassone)
        {

            List<VDizCassoniOutput> outputColata = new List<VDizCassoniOutput>();
            outputColata = await _context.VDizCassoniOutput.Where(oc => oc.VdcoTipoScheda == (int)Global.Globals.SchedaType.BASE && oc.VdcoIdCassone == cassone).ToListAsync();
            return outputColata.ToArray();
        }

        [HttpPost]
        [ActionName("GetDizOutputColataForniBacino")]
        public async Task<VDizCassoniOutput[]> GetDizOutputColataForniBacino([FromBody] int cassone)
        {

            List<VDizCassoniOutput> outputColata = new List<VDizCassoniOutput>();
            outputColata = await _context.VDizCassoniOutput.Where(oc => oc.VdcoTipoScheda == (int)Global.Globals.SchedaType.SPECIFICA && oc.VdcoIdCassone == cassone).ToListAsync();
            return outputColata.ToArray();
        }

        [HttpPost]
        [ActionName("GetDizOutputColate")]
        public async Task<DizOutputColata[]> GetDizOutputColate()
        {
            try
            {
                return await _context.DizOutputColata.ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetDizOutputColate");
                return null;
            }
        }

        [HttpPost]
        [ActionName("UpdateDizOutputColate")]
        public async Task<RetCode> UpdateDizOutputColate([FromBody] DizOutputColata param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                _context.DizOutputColata.Update(param);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "UpdateDizOutputColate");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        [HttpPost]
        [ActionName("InsertDizOutputColate")]
        public async Task<RetCode> InsertDizOutputColate([FromBody] DizOutputColata param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");
            int id;

            try
            {
                id = Convert.ToInt32(await _context.DizOutputColata.MaxAsync(doc => doc.DocId));
                param.DocId = (byte)(id + 1);
                await _context.DizOutputColata.AddAsync(param);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "InsertDizOutputColate");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        [HttpPost]
        [ActionName("GetOutputColataByOven")]
        public async Task<DizOutputColata[]> GetOutputColataByOven([FromBody] DizOutputColata param)
        {
            try
            {
                if (param.DocFb)
                {
                    if (param.DocIsColaticcio.HasValue && param.DocIsColaticcio.Value)
                        return await _context.DizOutputColata.Where(x => x.DocEnabled == true && x.DocFb == true && x.DocIsColaticcio.HasValue && x.DocIsColaticcio.Value).ToArrayAsync();
                    else
                        return await _context.DizOutputColata.Where(x => x.DocEnabled == true && x.DocFb == true).ToArrayAsync();
                }
                else
                {
                    if (param.DocIsColaticcio.HasValue && param.DocIsColaticcio.Value)
                        return await _context.DizOutputColata.Where(x => x.DocEnabled == true && x.DocFf == true && x.DocIsColaticcio.HasValue && x.DocIsColaticcio.Value).ToArrayAsync();
                    else
                        return await _context.DizOutputColata.Where(x => x.DocEnabled == true && x.DocFf == true).ToArrayAsync();
                }
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "GetOutputColataByOven");
                return null;
            }
        }

        [HttpPost]
        [ActionName("GetOutputColataBase")]
        public async Task<VOutputColataBase[]> GetOutputColataBase([FromBody] VColateAll param)
        {
            try
            {
                return await _context.VOutputColataBase.Where(x => x.VocIdColata == param.VcaId).OrderByDescending(x => x.VocDichiarazione).ToArrayAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "GetSchedeColaticciByOutput");
                return null;
            }
        }

        [HttpPost]
        [ActionName("GetOutputColataBaseByShift")]
        public async Task<VOutputColataBase[]> GetOutputColataBaseByShift([FromBody] Filter param)
        {
            try
            {
                return await _context.VOutputColataBase.Where(x => x.VocIdColata == param.IdColata && x.VocDichiarazione >= param.StartDate.ToUniversalTime() && x.VocDichiarazione <= param.EndDate.ToUniversalTime()).OrderByDescending(x => x.VocDichiarazione).ToArrayAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "GetSchedeColaticciByOutput");
                return null;
            }
        }

        [HttpPost]
        [ActionName("GetOutputColataSpecifica")]
        public async Task<VOutputColataSpecifica[]> GetOutputColataSpecifica([FromBody] VColateAll param)
        {
            try
            {
                return await _context.VOutputColataSpecifica.Where(x => x.VocIdColata == param.VcaId).OrderByDescending(x => x.VocDichiarazione).ToArrayAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "GetSchedeColaticciByOutput");
                return null;
            }
        }

        #endregion

        #region DIZIONARIO FERMI

        [HttpPost]
        [ActionName("GetDizFermiByType")]
        public async Task<VDizFermi[]> GetDizFermiByType([FromBody] string TipoFermo)
        {
            try
            {
                return await _context.VDizFermi.Where(vdf => vdf.VfTipoFermo == TipoFermo).OrderBy(vdf => vdf.VfDescrizione).ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetDizFermiByType");
                return null;
            }
        }

        [HttpPost]
        [ActionName("UpdateDizFermi")]
        public async Task<RetCode> UpdateDizFermi([FromBody] VDizFermi param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                switch (param.VfTipoFermo.ToUpper())
                {
                    case "A":
                        {
                            DizFermiA dfa = new DizFermiA();
                            dfa.DfaId = param.VfId;
                            dfa.DfaImpianto = param.VfIdImpianto;
                            dfa.DfaDescrizione = param.VfDescrizione;
                            _context.DizFermiA.Update(dfa);
                        }
                        break;
                    case "B":
                        {
                            DizFermiB dfb = new DizFermiB();
                            dfb.DfbId = param.VfId;
                            dfb.DfbImpianto = param.VfIdImpianto;
                            dfb.DfbDescrizione = param.VfDescrizione;
                            _context.DizFermiB.Update(dfb);
                        }
                        break;
                    case "C":
                        {
                            DizFermiC dfc = new DizFermiC();
                            dfc.DfcId = param.VfId;
                            dfc.DfcImpianto = param.VfIdImpianto;
                            dfc.DfcDescrizione = param.VfDescrizione;
                            _context.DizFermiC.Update(dfc);
                        }
                        break;
                    case "D":
                        {
                            DizFermiD dfd = new DizFermiD();
                            dfd.DfdId = param.VfId;
                            dfd.DfdImpianto = param.VfIdImpianto;
                            dfd.DfdDescrizione = param.VfDescrizione;
                            _context.DizFermiD.Update(dfd);
                        }
                        break;
                    case "E":
                        {
                            DizFermiE dfe = new DizFermiE();
                            dfe.DfeId = param.VfId;
                            dfe.DfeImpianto = param.VfIdImpianto;
                            dfe.DfeDescrizione = param.VfDescrizione;
                            _context.DizFermiE.Update(dfe);
                        }
                        break;
                    case "F":
                        {
                            DizFermiF dff = new DizFermiF();
                            dff.DffId = param.VfId;
                            dff.DffImpianto = param.VfIdImpianto;
                            dff.DffDescrizione = param.VfDescrizione;
                            _context.DizFermiF.Update(dff);
                        }
                        break;
                }
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "UpdateDizFermi");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        [HttpPost]
        [ActionName("InsertDizFermi")]
        public async Task<RetCode> InsertDizFermi([FromBody] VDizFermi param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");
            int id;

            try
            {
                switch (param.VfTipoFermo.ToUpper())
                {
                    case "A":
                        {
                            id = Convert.ToInt32(await _context.DizFermiA.MaxAsync(dfa => dfa.DfaId));
                            param.VfId = (byte)(id + 1);
                            DizFermiA dfa = new DizFermiA();
                            dfa.DfaId = param.VfId;
                            dfa.DfaImpianto = param.VfIdImpianto;
                            dfa.DfaDescrizione = param.VfDescrizione;
                            _context.DizFermiA.Add(dfa);
                        }
                        break;
                    case "B":
                        {
                            id = Convert.ToInt32(await _context.DizFermiB.MaxAsync(dfb => dfb.DfbId));
                            param.VfId = (byte)(id + 1);
                            DizFermiB dfb = new DizFermiB();
                            dfb.DfbId = param.VfId;
                            dfb.DfbImpianto = param.VfIdImpianto;
                            dfb.DfbDescrizione = param.VfDescrizione;
                            _context.DizFermiB.Add(dfb);
                        }
                        break;
                    case "C":
                        {
                            id = Convert.ToInt32(await _context.DizFermiC.MaxAsync(dfc => dfc.DfcId));
                            param.VfId = (byte)(id + 1);
                            DizFermiC dfc = new DizFermiC();
                            dfc.DfcId = param.VfId;
                            dfc.DfcImpianto = param.VfIdImpianto;
                            dfc.DfcDescrizione = param.VfDescrizione;
                            _context.DizFermiC.Add(dfc);
                        }
                        break;
                    case "D":
                        {
                            id = Convert.ToInt32(await _context.DizFermiD.MaxAsync(dfe => dfe.DfdId));
                            param.VfId = (byte)(id + 1);
                            DizFermiD dfd = new DizFermiD();
                            dfd.DfdId = param.VfId;
                            dfd.DfdImpianto = param.VfIdImpianto;
                            dfd.DfdDescrizione = param.VfDescrizione;
                            _context.DizFermiD.Add(dfd);
                        }
                        break;
                    case "E":
                        {
                            id = Convert.ToInt32(await _context.DizFermiE.MaxAsync(dfe => dfe.DfeId));
                            param.VfId = (byte)(id + 1);
                            DizFermiE dfe = new DizFermiE();
                            dfe.DfeId = param.VfId;
                            dfe.DfeImpianto = param.VfIdImpianto;
                            dfe.DfeDescrizione = param.VfDescrizione;
                            _context.DizFermiE.Add(dfe);
                        }
                        break;
                    case "F":
                        {
                            id = Convert.ToInt32(await _context.DizFermiF.MaxAsync(dff => dff.DffId));
                            param.VfId = (byte)(id + 1);
                            DizFermiF dff = new DizFermiF();
                            dff.DffId = param.VfId;
                            dff.DffImpianto = param.VfIdImpianto;
                            dff.DffDescrizione = param.VfDescrizione;
                            _context.DizFermiF.Add(dff);
                        }
                        break;
                }

                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "InsertDizFermi");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }


        #endregion

        #region DIZIONARIO MAPPA FERMI

        [HttpPost]
        [ActionName("GetDizMappaFermiByImpianto")]
        public async Task<VDizMappaFermi[]> GetDizMappaFermiByImpianto([FromBody] VDizMappaFermi mappaFermi)
        {
            try
            {
                return await _context.VDizMappaFermi.Where(vdmf => vdmf.VdmfImpianto == mappaFermi.VdmfImpianto).OrderBy(vdmf => vdmf.VdmfColonnaA).
                    ThenBy(vdmf => vdmf.VdmfColonnaB).ThenBy(vdmf => vdmf.VdmfColonnaC).ThenBy(vdmf => vdmf.VdmfColonnaD).ThenBy(vdmf => vdmf.VdmfColonnaE).
                    ThenBy(vdmf => vdmf.VdmfColonnaF).ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetDizMappaFermiByImpianto");
                return null;
            }
        }

        [HttpPost]
        [ActionName("InsertDizMappaFermi")]
        public async Task<RetCode> InsertDizMappaFermi([FromBody] DizMappaFermi param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                List<DizMappaFermi> mappa = await _context.DizMappaFermi.Where(dmf => dmf.DmfColonnaA == param.DmfColonnaA &&
                dmf.DmfColonnaB == param.DmfColonnaB && dmf.DmfColonnaC == param.DmfColonnaC && dmf.DmfColonnaD == param.DmfColonnaD &&
                dmf.DmfColonnaE == param.DmfColonnaE && dmf.DmfColonnaF == param.DmfColonnaF).ToListAsync();

                if (mappa.Count == 0)
                {
                    await _context.DizMappaFermi.AddAsync(param);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    rc.retCode = (int)Globals.RetCodes.NotOk;
                    rc.note = "La mappa che si vuole inserire è già esistente. Inserirne una nuova";
                }
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "InsertDizMappaFermi");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        [HttpPost]
        [ActionName("DeleteDizMappaFermi")]
        public async Task<RetCode> DeleteDizMappaFermi([FromBody] VDizMappaFermi param)
        {
            RetCode rRet = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                DizMappaFermi mappa = await _context.DizMappaFermi.Where(dmf => dmf.DmfId == param.VdmfId).FirstOrDefaultAsync();
                if (mappa != null)
                {
                    _context.DizMappaFermi.Remove(mappa);
                    _ = await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                rRet.retCode = (int)Globals.RetCodes.NotOk;
                Globals.g_Logger.Error(ex, "DeleteDizMappaFermi");
            }

            return rRet;
        }

        #endregion

        #region DIZIONARIO IMPIANTI RAPPORTI LAVORO

        [HttpPost]
        [ActionName("GetDizImpiantiRapportiLavoro")]
        public async Task<DizImpiantiRapportiLavoro[]> GetDizImpiantiRapportiLavoro()
        {
            try
            {
                return await _context.DizImpiantiRapportiLavoro.ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetDizImpiantiRapportiLavoro");
                return null;
            }
        }

        #endregion

        #region DIZIONARIO CASSONI

        [HttpPost]
        [ActionName("GetDizCassoni")]
        public async Task<DizCassoni[]> GetDizCassoni()
        {
            try
            {
                return await _context.DizCassoni.ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetDizCassoni");
                return null;
            }
        }

        [HttpPost]
        [ActionName("GetListCassoni")]
        public async Task<VListCassoni[]> GetListCassoni()
        {
            try
            {
                return await _context.VListCassoni.ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetListCassoni");
                return null;
            }
        }

        [HttpPost]
        [ActionName("GetDizCassoniByTipoScheda")]
        public async Task<DizCassoni[]> GetDizCassoniByTipoScheda([FromBody] int tiposcheda)
        {
            try
            {
                return await _context.DizCassoni.Where(dc => dc.DcaTipoScheda == tiposcheda).ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetDizCassoni");
                return null;
            }
        }

        [HttpPost]
        [ActionName("GetCassoneById")]
        public async Task<DizCassoni> GetCassoneById([FromBody] int idCassone)
        {
            try
            {
                return await _context.DizCassoni.Where(dca => dca.DcaId == idCassone).FirstAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetCassoneById");
                return null;
            }
        }

        [HttpPost]
        [ActionName("GetDizCassoniOuputByCassone")]
        public async Task<DizCassoniOutput[]> GetDizCassoniOuputByCassone([FromBody] int idCassone)
        {
            try
            {
                return await _context.DizCassoniOutput.Where(dco => dco.DcoIdCassone == idCassone).ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetDizCassoniOuputByCassone");
                return null;
            }
        }

        [HttpPost]
        [ActionName("UpdateDizCassoni")]
        public async Task<RetCode> UpdateDizCassoni([FromBody] DizCassoni param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                _context.DizCassoni.Update(param);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "UpdateDizCassoni");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        [HttpPost]
        [ActionName("InsertDizCassoni")]
        public async Task<RetCode> InsertDizCassoni([FromBody] DizCassoni param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");
            int id;

            try
            {
                id = Convert.ToInt32(await _context.DizCassoni.MaxAsync(dca => dca.DcaId));
                param.DcaId = (byte)(id + 1);
                rc.note = param.DcaId.ToString();
                await _context.DizCassoni.AddAsync(param);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "InsertDizCassoni");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        [HttpPost]
        [ActionName("UpdateDizCassoniOutput")]
        public async Task<RetCode> UpdateDizCassoniOutput([FromBody] DizCassoniOutput[] param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                //_context.DizOutputColata.Update(param);
                DizCassoniOutput[] toRemove = await _context.DizCassoniOutput.Where(dco => dco.DcoIdCassone == param[0].DcoIdCassone).ToArrayAsync();
                _context.DizCassoniOutput.RemoveRange(toRemove);
                _context.DizCassoniOutput.AddRange(param);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "UpdateDizCassoniOutput");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        #endregion

        #region DIZIONARIO TIPO SCHEDA

        [HttpPost]
        [ActionName("GetDizTipoScheda")]
        public async Task<DizTipoScheda[]> GetDizTipoScheda()
        {
            try
            {
                return await _context.DizTipoScheda.ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetDizTipoScheda");
                return null;
            }
        }

        #endregion

        #region DIZIONARIO ANOMALIE

        [HttpPost]
        [ActionName("GetListAnomalie")]
        public async Task<DizAnomalie[]> GetListAnomalie()
        {
            try
            {
                return await _context.DizAnomalie.OrderBy(da => da.DanNome).ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetListAnomalie");
                return null;
            }
        }

        [HttpPost]
        [ActionName("GetListAnomalieEnabled")]
        public async Task<DizAnomalie[]> GetListAnomalieEnabled()
        {
            try
            {
                return await _context.DizAnomalie.Where(dan => dan.DanEnabled.Value).OrderBy(da => da.DanNome).ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetListAnomalieEnabled");
                return null;
            }
        }

        [HttpPost]
        [ActionName("UpdateDizAnomalia")]
        public async Task<RetCode> UpdateDizAnomalia([FromBody] DizAnomalie param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                _context.DizAnomalie.Update(param);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "UpdateDizAnomalia");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        [HttpPost]
        [ActionName("InsertDizAnomalia")]
        public async Task<RetCode> InsertDizAnomalia([FromBody] DizAnomalie param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");
            int id;
            try
            {
                if (await _context.DizAnomalie.CountAsync() == 0)
                    id = 0;
                else
                    id = Convert.ToInt32(await _context.DizAnomalie.MaxAsync(dan => dan.DanId));
                param.DanId = (byte)(id + 1);
                await _context.DizAnomalie.AddAsync(param);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "InsertDizAnomalia");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        #endregion

        #region DIZIONARIO MATERIE PRIME

        [HttpPost]
        [ActionName("GetListCorrettiviEnabled")]
        public async Task<MateriePrime[]> GetListCorrettiviEnabled()
        {
            try
            {
                return await _context.MateriePrime.Where(mp => mp.MpInUse.Value && mp.MpTipoMateriale == (int)Globals.TipoMateriale.CORRETTIVO).ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetListCorrettiviEnabled");
                return null;
            }
        }

        [HttpPost]
        [ActionName("GetListRottammiEnabled")]
        public async Task<MateriePrime[]> GetListRottammiEnabled()
        {
            try
            {
                return await _context.MateriePrime.Where(mp => mp.MpInUse.Value && mp.MpTipoMateriale == (int)Globals.TipoMateriale.ROTTAME).ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetListCorrettiviEnabled");
                return null;
            }
        }

        [HttpPost]
        [ActionName("GetListMaterie")]
        public async Task<VMateriePrime[]> GetListMaterie()
        {
            try
            {
                return await _context.VMateriePrime.ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetListMaterie");
                return null;
            }
        }

        [HttpPost]
        [ActionName("GetListMaterieWeb")]
        public async Task<VMateriePrimeWeb[]> GetListMaterieWeb()
        {
            try
            {
                return await _context.VMateriePrimeWeb.ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetListMaterieWeb");
                return null;
            }
        }

        [HttpPost]
        [ActionName("GetListMaterieWebByTipologia")]
        public async Task<VMateriePrimeWeb[]> GetListMaterieWebByTipologia([FromBody] DizTipiMateriale tipologia)
        {
            try
            {
                return await _context.VMateriePrimeWeb.Where(vmpw => vmpw.VmpDtmId == tipologia.DtmId).ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetListMaterieWebByTipologia");
                return null;
            }
        }

        [HttpPost]
        [ActionName("GetListAssociazioni")]
        public async Task<VElementiChimiciMateriePrime[]> GetListAssociazioni()
        {
            try
            {
                return await _context.VElementiChimiciMateriePrime.ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetListAssociazioni");
                return null;
            }
        }

        [HttpPost]
        [ActionName("GetAssociazioniByMateria")]
        public async Task<VElementiChimiciMateriePrime[]> GetAssociazioniByMateria([FromBody] VMateriePrimeWeb materia)
        {
            try
            {
                return await _context.VElementiChimiciMateriePrime.Where(vecmp => vecmp.VecmpIdMateriaPrima == materia.VmpId).ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetAssociazioniByMateria");
                return null;
            }
        }

        [HttpPost]
        [ActionName("UpdateMateria")]
        public async Task<RetCode> UpdateMateria([FromBody] MateriePrime param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                _context.MateriePrime.Update(param);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "UpdateMateria");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        [HttpPost]
        [ActionName("InsertMateria")]
        public async Task<RetCode> InsertMateria([FromBody] MateriePrime param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");
            int id;
            try
            {
                if ((await _context.MateriePrime.Where(mp => mp.MpCodice == param.MpCodice).CountAsync() == 0))
                {
                    await _context.MateriePrime.AddAsync(param);
                    _ = await _context.SaveChangesAsync();
                }
                else
                    rc.retCode = (int)Globals.RetCodes.NotOk;
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "InsertMaterie");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        [HttpPost]
        [ActionName("SaveAssociazioni")]
        public async Task<RetCode> SaveAssociazioni([FromBody] ElementiChimiciMateriePrime[] param)
        {
            RetCode ret = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                List<ElementiChimiciMateriePrime> associazioniList = await _context.ElementiChimiciMateriePrime.Where(x => x.EcmpIdMateriaPrima == param[0].EcmpIdMateriaPrima).ToListAsync();
                _context.ElementiChimiciMateriePrime.RemoveRange(associazioniList);
                _ = _context.SaveChanges();

                await _context.ElementiChimiciMateriePrime.AddRangeAsync(param);
                _ = await _context.SaveChangesAsync();

                return ret;
            }
            catch (Exception ex)
            {
                ret = new RetCode((int)Globals.RetCodes.NotOk, ex.Message);
                Globals.g_Logger.Error(ex, "SaveSecurity");
                return ret;
            }
        }


        #endregion

        #region DIZIONARIO STATI ANALISI

        [HttpPost]
        [ActionName("GetListStatiAnalisi")]
        public async Task<DizStatiAnalisi[]> GetListStatiAnalisi()
        {
            try
            {
                return await _context.DizStatiAnalisi.ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetListStatiAnalisi");
                return null;
            }
        }

        [HttpPost]
        [ActionName("UpdateDizStatoAnalisi")]
        public async Task<RetCode> UpdateDizStatoAnalisi([FromBody] DizStatiAnalisi param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                _context.DizStatiAnalisi.Update(param);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "UpdateDizStatoAnalisi");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        [HttpPost]
        [ActionName("InsertDizStatoAnalisi")]
        public async Task<RetCode> InsertDizStatoAnalisi([FromBody] DizStatiAnalisi param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");
            int id;
            try
            {
                await _context.DizStatiAnalisi.AddAsync(param);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "InsertDizStatoAnalisi");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        #endregion

        #region DIZIONARIO ELEMENTI CHIMICI

        [HttpPost]
        [ActionName("GetListElementiChimici")]
        public async Task<VElementiChimici[]> GetListElementiChimici()
        {
            try
            {
                return await _context.VElementiChimici.Where(ec => ec.VecDtmId == (int)Globals.TipoMateriale.ELEMENTI_CHIMICI).ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetListElementiChimici");
                return null;
            }
        }

        [HttpPost]
        [ActionName("UpdateElementoChimico")]
        public async Task<RetCode> UpdateElementoChimico([FromBody] ElementiChimici param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                _context.ElementiChimici.Update(param);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "UpdateElementoChimico");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        [HttpPost]
        [ActionName("InsertElementoChimico")]
        public async Task<RetCode> InsertElementoChimico([FromBody] ElementiChimici param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");
            int id;
            try
            {
                await _context.ElementiChimici.AddAsync(param);
                _ = await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "InsertElementoChimico");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        #endregion

        #region DIZIONARIO GRUPPI UTENTE

        [HttpPost]
        [ActionName("GetListGruppiUtente")]
        public async Task<DizGruppiUtente[]> GetListGruppiUtente()
        {
            try
            {
                return await _context.DizGruppiUtente.ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetListGruppiUtente");
                return null;
            }
        }

        [HttpPost]
        [ActionName("UpdateDizGruppoUtente")]
        public async Task<RetCode> UpdateDizGruppoUtente([FromBody] DizGruppiUtente param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                _context.DizGruppiUtente.Update(param);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "UpdateDizGruppoUtente");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        [HttpPost]
        [ActionName("InsertDizGruppoUtente")]
        public async Task<RetCode> InsertDizGruppoUtente([FromBody] DizGruppiUtente param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");
            int id;

            try
            {
                await _context.DizGruppiUtente.AddAsync(param);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "InsertDizGruppoUtente");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        #endregion

        #region DIZIONARIO GRUPPI PRODOTTO

        [HttpPost]
        [ActionName("GetListGruppiProdotto")]
        public async Task<DizGruppiProdotto[]> GetListGruppiProdotto()
        {
            try
            {
                return await _context.DizGruppiProdotto.ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetListGruppiProdotto");
                return null;
            }
        }

        [HttpPost]
        [ActionName("UpdateDizGruppiProdotto")]
        public async Task<RetCode> UpdateDizGruppiProdotto([FromBody] DizGruppiProdotto param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                _context.DizGruppiProdotto.Update(param);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "UpdateDizGruppiProdotto");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        [HttpPost]
        [ActionName("InsertDizGruppiProdotto")]
        public async Task<RetCode> InsertDizGruppiProdotto([FromBody] DizGruppiProdotto param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");
            int id;

            try
            {
                await _context.DizGruppiProdotto.AddAsync(param);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "InsertDizGruppiProdotto");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        #endregion

        #region DIZIONARIO PEZZATURE

        [HttpPost]
        [ActionName("GetListPezzature")]
        public async Task<DizPezzatura[]> GetListPezzature()
        {
            try
            {
                return await _context.DizPezzatura.ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetListPezzature");
                return null;
            }
        }

        [HttpPost]
        [ActionName("UpdateDizPezzatura")]
        public async Task<RetCode> UpdateDizPezzatura([FromBody] DizPezzatura param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                _context.DizPezzatura.Update(param);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "UpdateDizPezzatura");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        [HttpPost]
        [ActionName("InsertDizPezzatura")]
        public async Task<RetCode> InsertDizPezzatura([FromBody] DizPezzatura param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                if (_context.DizPezzatura.Count(dpe => dpe.DpeId == param.DpeId) != 0)
                    rc.retCode = (int)Globals.RetCodes.NotOk;
                else
                {
                    await _context.DizPezzatura.AddAsync(param);
                    _ = await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "InsertDizPezzatura");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        #endregion

        #region DIZIONARIO FAMIGLIE

        [HttpPost]
        [ActionName("GetListFamiglie")]
        public async Task<DizFamiglia[]> GetListFamiglie()
        {
            try
            {
                return await _context.DizFamiglia.ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetListFamiglie");
                return null;
            }
        }

        [HttpPost]
        [ActionName("UpdateDizFamiglie")]
        public async Task<RetCode> UpdateDizFamiglie([FromBody] DizFamiglia param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                _context.DizFamiglia.Update(param);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "UpdateDizFamiglie");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        [HttpPost]
        [ActionName("InsertDizFamiglie")]
        public async Task<RetCode> InsertDizFamiglie([FromBody] DizFamiglia param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                await _context.DizFamiglia.AddAsync(param);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "InsertDizFamiglie");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }


        #endregion 

        #region DIZIONARIO FORMATI

        [HttpPost]
        [ActionName("GetListFormati")]
        public async Task<DizFormato[]> GetListFormati()
        {
            try
            {
                return await _context.DizFormato.ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetListFormati");
                return null;
            }
        }

        #endregion

        #region DIZIONARIO PROVENIENZE

        [HttpPost]
        [ActionName("GetListProvenienze")]
        public async Task<DizProvenienza[]> GetListProvenienze()
        {
            try
            {
                return await _context.DizProvenienza.ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetListProvenienze");
                return null;
            }
        }

        [HttpPost]
        [ActionName("UpdateDizProvenienza")]
        public async Task<RetCode> UpdateDizProvenienza([FromBody] DizProvenienza param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                _context.DizProvenienza.Update(param);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "UpdateDizProvenienza");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        [HttpPost]
        [ActionName("InsertDizProvenienza")]
        public async Task<RetCode> InsertDizProvenienza([FromBody] DizProvenienza param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                await _context.DizProvenienza.AddAsync(param);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "InsertDizProvenienza");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        #endregion

        #region DIZIONARIO TIPI MATERIALE

        [HttpPost]
        [ActionName("GetListTipiMateriale")]
        public async Task<DizTipiMateriale[]> GetListTipiMateriale()
        {
            try
            {
                return await _context.DizTipiMateriale.ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetListTipiMateriale");
                return null;
            }
        }

        [HttpPost]
        [ActionName("UpdateDizTipiMateriale")]
        public async Task<RetCode> UpdateDizTipiMateriale([FromBody] DizTipiMateriale param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                _context.DizTipiMateriale.Update(param);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "UpdateDizTipiMateriale");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        [HttpPost]
        [ActionName("InsertDizTipiMateriale")]
        public async Task<RetCode> InsertDizTipiMateriale([FromBody] DizTipiMateriale param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");
            int id;

            try
            {
                await _context.DizTipiMateriale.AddAsync(param);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "InsertDizTipiMateriale");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        #endregion

        #region DIZIONARIO QUALITA COLATICCIO

        [HttpPost]
        [ActionName("GetListQualitaColaticcio")]
        public async Task<DizQualitaColaticcio[]> GetListQualitaColaticcio()
        {
            try
            {
                return await _context.DizQualitaColaticcio.ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetListQualitaColaticcio");
                return null;
            }
        }

        [HttpPost]
        [ActionName("GetListQualitaColaticcioEnabled")]
        public async Task<DizQualitaColaticcio[]> GetListQualitaColaticcioEnabled()
        {
            try
            {
                return await _context.DizQualitaColaticcio.Where(dqc => dqc.DqcEnabled.Value).ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetListQualitaColaticcioEnabled");
                return null;
            }
        }

        [HttpPost]
        [ActionName("UpdateDizQualitaColaticcio")]
        public async Task<RetCode> UpdateDizQualitaColaticcio([FromBody] DizQualitaColaticcio param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                _context.DizQualitaColaticcio.Update(param);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "UpdateDizQualitaColaticcio");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        [HttpPost]
        [ActionName("InsertDizQualitaColaticcio")]
        public async Task<RetCode> InsertDizQualitaColaticcio([FromBody] DizQualitaColaticcio param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");
            int id;

            try
            {
                if (await _context.DizQualitaColaticcio.CountAsync() == 0)
                    id = 0;
                else
                    id = Convert.ToInt32(await _context.DizQualitaColaticcio.MaxAsync(dqc => dqc.DqcId));
                param.DqcId = (byte)(id + 1);
                await _context.DizQualitaColaticcio.AddAsync(param);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "InsertDizQualitaColaticcio");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        #endregion

        #region DIZIONARIO MACCHINE
        [HttpPost]
        [ActionName("GetListMacchineEnabled")]
        public async Task<DizMacchine[]> GetListMacchineEnabled()
        {
            try
            {
                return await _context.DizMacchine.Where(dm => dm.DmEnabled.Value).ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetListAnomalieEnabled");
                return null;
            }
        }

        [HttpPost]
        [ActionName("GetListMacchine")]
        public async Task<DizMacchine[]> GetListMacchine()
        {
            try
            {
                return await _context.DizMacchine.ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetListMacchine");
                return null;
            }
        }

        [HttpPost]
        [ActionName("UpdateDizMacchine")]
        public async Task<RetCode> UpdateDizMacchine([FromBody] DizMacchine param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                _context.DizMacchine.Update(param);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "UpdateDizMacchine");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        [HttpPost]
        [ActionName("InsertDizMacchine")]
        public async Task<RetCode> InsertDizMacchine([FromBody] DizMacchine param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                if (_context.DizMacchine.Count() == 0)
                    param.DmId = 0;
                else
                    param.DmId = await _context.DizMacchine.MaxAsync(dm => dm.DmId);
                param.DmId++;

                await _context.DizMacchine.AddAsync(param);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "InsertDizMacchine");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }


        #endregion

        #region DIZIONARIO CAUSE RALLENTAMENTI
        [HttpPost]
        [ActionName("GetListCauseRallentamenti")]
        public async Task<DizCauseRallentamenti[]> GetListCauseRallentamenti()
        {
            try
            {
                return await _context.DizCauseRallentamenti.OrderBy(dcr => dcr.DcrDescrizione).ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetListCauseRallentamentiEnabled");
                return null;
            }
        }

        [HttpPost]
        [ActionName("GetListCauseRallentamentiEnabled")]
        public async Task<DizCauseRallentamenti[]> GetListCauseRallentamentiEnabled()
        {
            try
            {
                return await _context.DizCauseRallentamenti.Where(dcr => dcr.DcrEnabled.Value).OrderBy(dcr => dcr.DcrDescrizione).ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetListCauseRallentamentiEnabled");
                return null;
            }
        }

        [HttpPost]
        [ActionName("UpdateDizCauseRallentamento")]
        public async Task<RetCode> UpdateDizCauseRallentamento([FromBody] DizCauseRallentamenti param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                _context.DizCauseRallentamenti.Update(param);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "UpdateDizCauseRallentamento");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        [HttpPost]
        [ActionName("InsertDizCauseRallentamento")]
        public async Task<RetCode> InsertDizCauseRallentamento([FromBody] DizCauseRallentamenti param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");
            int id;
            try
            {
                await _context.DizCauseRallentamenti.AddAsync(param);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "InsertDizCauseRallentamento");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        #endregion

        #region DIZIONARIO PROVENIENZA COLATICCIO
        [HttpPost]
        [ActionName("GetListProvenienzaColaticcio")]
        public async Task<DizProvenienzaColaticcio[]> GetListProvenienzaColaticcio()
        {
            try
            {
                return await _context.DizProvenienzaColaticcio.ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetListProvenienzaColaticcio");
                return null;
            }
        }

        [HttpPost]
        [ActionName("GetListProvenienzaColaticcioEnabled")]
        public async Task<DizProvenienzaColaticcio[]> GetListProvenienzaColaticcioEnabled()
        {
            try
            {
                return await _context.DizProvenienzaColaticcio.Where(dpc => dpc.DpcEnabled.Value).ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetListProvenienzaColaticcioEnabled");
                return null;
            }
        }

        [HttpPost]
        [ActionName("UpdateDizProvenienzaColaticcio")]
        public async Task<RetCode> UpdateDizProvenienzaColaticcio([FromBody] DizProvenienzaColaticcio param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                _context.DizProvenienzaColaticcio.Update(param);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "UpdateDizProvenienzaColaticcio");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        [HttpPost]
        [ActionName("InsertDizProvenienzaColaticcio")]
        public async Task<RetCode> InsertDizProvenienzaColaticcio([FromBody] DizProvenienzaColaticcio param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");
            int id;

            try
            {
                if (await _context.DizProvenienzaColaticcio.CountAsync() == 0)
                    id = 0;
                else
                    id = Convert.ToInt32(await _context.DizProvenienzaColaticcio.MaxAsync(dpc => dpc.DpcId));
                param.DpcId = (byte)(id + 1);
                await _context.DizProvenienzaColaticcio.AddAsync(param);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "InsertDizProvenienzaColaticcio");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        #endregion

        #region DIZIONARIO OPERATORI
        [HttpPost]
        [ActionName("GetListOperatori")]
        public async Task<Operatori[]> GetListOperatori()
        {
            try
            {
                return await _context.Operatori.ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetListOperatori");
                return null;
            }
        }

        [HttpPost]
        [ActionName("GetListOperatoriEnabled")]
        public async Task<Operatori[]> GetListOperatoriEnabled()
        {
            try
            {
                return await _context.Operatori.Where(o => o.OEnabled.Value).ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetListOperatoriEnabled");
                return null;
            }
        }

        [HttpPost]
        [ActionName("UpdateOperatore")]
        public async Task<RetCode> UpdateOperatore([FromBody] Operatori param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                _context.Operatori.Update(param);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "UpdateOperatore");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        [HttpPost]
        [ActionName("InsertOperatore")]
        public async Task<RetCode> InsertOperatore([FromBody] Operatori param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");

            try
            {
                List<Operatori> op = new List<Operatori>();
                op = await _context.Operatori.Where(o => o.OUsername == param.OUsername).ToListAsync();
                if (op.Count == 0)
                {
                    int id = await _context.Operatori.MaxAsync(o => o.OId);
                    param.OId = id + 1;
                    await _context.Operatori.AddAsync(param);
                    _ = await _context.SaveChangesAsync();
                }
                else
                {
                    rc.retCode = (int)Globals.RetCodes.NotOk;
                    rc.note = "E' già presente un utente con username " + param.OUsername;
                }
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "InsertOperatore");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        #endregion

        [HttpPost]
        [ActionName("GetSchedeColaticciByOutput")]
        public async Task<VOutForniCassoni[]> GetSchedeColaticciByOutput([FromBody] DizOutputColata param)
        {
            try
            {
                if (param.DocFb)
                    return await _context.VOutForniCassoni.Where(x => x.VofcIdOutput == param.DocId && x.VofcTipoSchedaOut.Value == 2).ToArrayAsync();
                else
                    return await _context.VOutForniCassoni.Where(x => x.VofcIdOutput == param.DocId && x.VofcTipoSchedaOut.Value == 1).ToArrayAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "GetSchedeColaticciByOutput");
                return null;
            }
        }

        [HttpPost]
        [ActionName("GetColateBySchedaOutput")]
        public async Task<VColateAll[]> GetColateBySchedaOutput([FromBody] DizOutputColata param)
        {
            try
            {
                if (param.DocFb)
                    return await _context.VColateAll.Where(x => x.VcaIsSpecifica.Value).OrderByDescending(x => x.VcaInizioOper).ToArrayAsync();
                else
                    return await _context.VColateAll.Where(x => x.VcaIsBase.Value).OrderByDescending(x => x.VcaInizioOper).ToArrayAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "GetSchedeColaticciByOutput");
                return null;
            }
        }

        [HttpPost]
        [ActionName("GetColateBySchedaOutputAndDate")]
        public async Task<VColateAll[]> GetColateBySchedaOutputAndDate([FromBody] Filter param)
        {
            try
            {
                if (param.TipoScheda.DocFb)
                    return await _context.VColateAll.Where(x => x.VcaIsSpecifica.Value && x.VcaInizioOper >= param.StartDate &&
                        x.VcaInizioOper <= param.EndDate).OrderByDescending(x => x.VcaInizioOper).ToArrayAsync();
                else
                    return await _context.VColateAll.Where(x => x.VcaIsBase.Value && x.VcaInizioOper >= param.StartDate &&
                        x.VcaInizioOper <= param.EndDate).OrderByDescending(x => x.VcaInizioOper).ToArrayAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "GetColateBySchedaOutputAndDate");
                return null;
            }
        }

        [HttpPost]
        [ActionName("GetForniBacino")]
        public async Task<DizImpianti[]> GetForniBacino()
        {
            try
            {
                return await _context.DizImpianti.Where(di => di.DiSiglaHmi.ToUpper().StartsWith("FB")).OrderBy(di => di.DiCodice).ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetForniBacino");
                return null;
            }
        }

        [HttpPost]
        [ActionName("GetForni")]
        public async Task<DizImpianti[]> GetForni()
        {
            try
            {
                return await _context.DizImpianti.Where(di => di.DiSiglaHmi.ToUpper().StartsWith("FB") || di.DiSiglaHmi.ToUpper().StartsWith("FF")).OrderBy(di => di.DiCodice).ToArrayAsync();
            }
            catch (Exception ex)
            {
                Global.Globals.g_Logger.Error(ex, "GetForniBacino");
                return null;
            }
        }
    }
}
