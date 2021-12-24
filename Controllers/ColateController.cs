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
    public class ColateController : Controller
    {

        private readonly RFFContext _context;

        public ColateController(RFFContext context)
        {
            _context = context;
        }


        [HttpPost]
        [ActionName("GetActualColataFornoFusorio")]
        public async Task<VColateAll> GetActualColataFornoFusorio()
        {
            VColateAll colataBase;
            colataBase = await _context.VColateAll.Where(cb => cb.VcaIdStato == 3 && cb.VcaIsBase.Value).FirstOrDefaultAsync();
            return colataBase;
        }

        [HttpPost]
        [ActionName("GetActualColataForniBacino")]
        public async Task<VColateAll> GetActualColataForniBacino([FromBody] DizImpianti impianto)
        {
            VColateAll colataSpecifica;
            colataSpecifica = await _context.VColateAll.Where(cs => cs.VcaIdStato == 3 && cs.VcaIdDestinazione == impianto.DiCodice).FirstOrDefaultAsync();
            return colataSpecifica;
        }

        [HttpPost]
        [ActionName("AddPesoColata")]
        public async Task<RetCode> AddPesoColata([FromBody] VColateAll colata)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");
            int id;

            try
            {
                if (colata.VcaIsBase.Value)
                {

                    ColateLegaBase clb = await _context.ColateLegaBase.Where(c => c.ClbId == colata.VcaId).FirstOrDefaultAsync();

                    clb.ClbPeso = colata.VcaPeso;

                    _context.ColateLegaBase.Update(clb);
                }
                else
                {

                    ColateLegaSpecifica cls = await _context.ColateLegaSpecifica.Where(c => c.ClsId == colata.VcaId).FirstOrDefaultAsync();

                    cls.ClsPeso = colata.VcaPeso;

                    _context.ColateLegaSpecifica.Update(cls);
                }
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "AddPesoColata");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        [HttpPost]
        [ActionName("GetColateBaseByDate")]
        public async Task<VColateBase[]> GetColateBaseByDate([FromBody] Filter filter)
        {

            List<VColateBase> colateBase = new List<VColateBase>();
            colateBase = await _context.VColateBase.Where(cb => cb.VcbInizioOper.Value.Date >= filter.StartDate.Date && cb.VcbInizioOper.Value.Date <= filter.EndDate.Date && (cb.VcbIdStato == 3 || cb.VcbIdStato == 4)).OrderByDescending(cb => cb.VcbInizioOper).ToListAsync();
            return colateBase.ToArray();
        }

        [HttpPost]
        [ActionName("GetColateBase")]
        public async Task<VColateBase[]> GetColateBase()
        {

            List<VColateBase> colateBase = new List<VColateBase>();
            colateBase = await _context.VColateBase.OrderBy(cb => cb.VcbInizioSched).ToListAsync();
            return colateBase.ToArray();
        }

        [HttpPost]
        [ActionName("GetColataBaseByRapportoBase")]
        public async Task<VColateBase> GetColataBaseByRapportoBase([FromBody] VRapportiLavoroBase rapp)
        {

            VColateBase colataBase;
            colataBase = await _context.VColateBase.Where(cb => cb.VcbId == rapp.VrlbId).FirstOrDefaultAsync();
            return colataBase;
        }

        [HttpPost]
        [ActionName("GetColateSpecificheByBase")]
        public async Task<VColateSpecifiche[]> GetColateSpecificheByBase([FromBody] int idBase)
        {

            List<VColateSpecifiche> colateSpecifiche = new List<VColateSpecifiche>();
            colateSpecifiche = await _context.VColateSpecifiche.Where(cs => cs.VcsIdColataLegaBase == idBase && (cs.VcsIdStato == 3 || cs.VcsIdStato == 4)).OrderByDescending(cs => cs.VcsInizioOper).ToListAsync();
            return colateSpecifiche.ToArray();
        }


        [HttpPost]
        [ActionName("GetColateSpecifiche")]
        public async Task<VColateSpecifiche[]> GetColateSpecifiche()
        {

            List<VColateSpecifiche> colateSpecifiche = new List<VColateSpecifiche>();
            colateSpecifiche = await _context.VColateSpecifiche.OrderBy(cs => cs.VcsInizioSched).ToListAsync();
            return colateSpecifiche.ToArray();
        }

        [HttpPost]
        [ActionName("GetColataSpecificaByRapporto")]
        public async Task<VColateSpecifiche> GetColataSpecificaByRapporto([FromBody] VRapportiLavoroSpecifici rapp)
        {

            VColateSpecifiche colataSpecifica;
            colataSpecifica = await _context.VColateSpecifiche.Where(cs => cs.VcsId == rapp.VrlsId).FirstOrDefaultAsync();
            return colataSpecifica;
        }

        [HttpPost]
        [ActionName("GetMateriaPrimeByID")]
        public async Task<VMateriePrime[]> GetMateriaPrimeByID([FromBody] int idMP)
        {

            List<VMateriePrime> materiePrime = new List<VMateriePrime>();
            materiePrime = await _context.VMateriePrime.Where(mp => mp.VmpId == idMP).ToListAsync();
            return materiePrime.ToArray();
        }

        [HttpPost]
        [ActionName("GetLegheBaseByColata")]
        public async Task<VLegheBase[]> GetLegheBaseByColata([FromBody] VColateBase colataBase)
        {
            List<VLegheBase> legheBase = new List<VLegheBase>();
            legheBase = await _context.VLegheBase.Where(lb => lb.LmbId == colataBase.VcbIdLega).ToListAsync();
            return legheBase.ToArray();
        }

        [HttpPost]
        [ActionName("GetLegheSpecificheByColata")]
        public async Task<VLegheSpecifiche[]> GetLegheSpecificheByColata([FromBody] VColateSpecifiche colataSpecifiche)
        {
            List<VLegheSpecifiche> legheSpecifiche = new List<VLegheSpecifiche>();
            legheSpecifiche = await _context.VLegheSpecifiche.Where(ls => ls.LmsId == colataSpecifiche.VcsIdLega).ToListAsync();
            return legheSpecifiche.ToArray();
        }

        [HttpPost]
        [ActionName("GetMaterieSchedulateByLega")]
        public async Task<VMaterieLegheAllWeb[]> GetMaterieSchedulateByLega([FromBody] VMaterieLegheAllWeb param)
        {
            try
            {
                return await _context.VMaterieLegheAllWeb.Where(x => x.VmlawIsBase == param.VmlawIsBase && x.VmlawIdColata == param.VmlawIdColata && x.VmlawQta > 0).ToArrayAsync();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "GetMaterieSchedulateByLega");
                return null;
            }
        }

    }
}
