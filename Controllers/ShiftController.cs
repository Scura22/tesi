using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RapportiLavoro.Global;
using RapportiLavoro.Models;
using RapportiLavoro.Models.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RapportiLavoro.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ShiftController : Controller
    {
        private readonly RFFContext _context;

        public ShiftController(RFFContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ActionName("AddOperatoreByUser")]
        public async Task<User> AddOperatoreByUser([FromBody] User us)
        {
            Operatori op;
            int id;
            op = await _context.Operatori.Where(op => op.OUsername == us.username).FirstOrDefaultAsync();
            if (op == null)
            {

                Operatori o = new Operatori();
                op = await _context.Operatori.OrderByDescending(x => x.OId).FirstOrDefaultAsync();
                if (op == null || op.OId < 100000)
                    o.OId = 100001;
                else
                    o.OId = op.OId + 1;

                o.ODtId = 1;
                o.OEnabled = true;
                o.OFullName = us.firstName + " " + us.lastName;
                o.OUsername = us.username;

                _context.Operatori.Add(o);
                _context.SaveChanges();

                us.idOperatore = o.OId;

                return us;

            }
            else
            {
                us.idOperatore = op.OId;
                return us;
            }
        }

        [HttpPost]
        [ActionName("GetActualShift")]
        public async Task<TurnoOperatori> GetActualShift([FromBody] TurnoOperatori param)
        {
            DateTime actStart, actEnd;
            short actTurn;

            try
            {
                Operatori op = await _context.Operatori.Where(o => o.OId == param.ToOId).FirstOrDefaultAsync();
                (actStart, actEnd, actTurn) = Utilities.CalculateTurn(op.ODtId.Value);


                TurnoOperatori es = await _context.TurnoOperatori.Where(to => to.ToOId.Value == op.OId && (to.ToStartDate.Value == actStart && to.ToEndDate.Value == actEnd) && to.ToTurnNumber == actTurn).FirstOrDefaultAsync();

                if (es == null)
                {
                    es = new TurnoOperatori
                    {
                        ToOId = op.OId,
                        ToStartDate = actStart,
                        ToEndDate = actEnd,
                        ToTurnNumber = actTurn,
                        ToLastLogin = DateTime.Now,
                        ToIdImpianto = param.ToIdImpianto
                    };
                    _ = await _context.TurnoOperatori.AddAsync(es);
                    int i = await _context.SaveChangesAsync();
                }
                else
                {
                    es.ToLastLogin = DateTime.Now;
                    _ = _context.TurnoOperatori.Update(es);
                    int i = await _context.SaveChangesAsync();
                }

                if (es != null)
                    Globals.g_Logger.Info("### Turno Operatore ID: " + es.ToId + " - ID Operatore Numero: " + es.ToOId);

                es.ToO = null;

                return es;
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, MethodBase.GetCurrentMethod().Name);
                return null;
            }
        }

        [HttpPost]
        [ActionName("GetPreviousShift")]
        public async Task<TurnoOperatori> GetPreviousShift([FromBody] TurnoOperatori param)
        {
            DateTime actStart, actEnd;
            short actTurn;

            try
            {
                Operatori op = await _context.Operatori.Where(o => o.OId == param.ToOId).FirstOrDefaultAsync();
                (actStart, actEnd, actTurn) = Utilities.CalculateTurn(op.ODtId.Value, true);


                TurnoOperatori es = await _context.TurnoOperatori.Where(to => to.ToOId.Value == op.OId && (to.ToStartDate.Value == actStart && to.ToEndDate.Value == actEnd) && to.ToTurnNumber == actTurn).FirstOrDefaultAsync();

                if (es == null)
                {
                    es = new TurnoOperatori
                    {
                        ToOId = op.OId,
                        ToStartDate = actStart,
                        ToEndDate = actEnd,
                        ToTurnNumber = actTurn,
                    };
                    //_ = await _context.TurnoOperatori.AddAsync(es);
                    //int i = await _context.SaveChangesAsync();
                }

                //if (es != null)
                //    Globals.g_Logger.Info("### Turno Operatore ID: " + es.ToId + " - ID Operatore Numero: " + es.ToOId);

                es.ToO = null;

                return es;
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, MethodBase.GetCurrentMethod().Name);
                return null;
            }
        }


        [HttpPost]
        [ActionName("GetOperatoriASupporto")]
        public async Task<Operatori[]> GetOperatoriASupporto([FromBody] TurnoOperatori param)
        {
            DateTime actStart, actEnd;
            short actTurn;

            List<Operatori> operatoriASupporto = new List<Operatori>();

            try
            {
                Operatori op = await _context.Operatori.Where(o => o.OId == param.ToOId).FirstOrDefaultAsync();
                (actStart, actEnd, actTurn) = Utilities.CalculateTurn(op.ODtId.Value);


                List<int?> es = await _context.TurnoOperatori.Where(to => (to.ToStartDate.Value == actStart && to.ToEndDate.Value == actEnd) &&
                to.ToTurnNumber == actTurn && to.ToIdImpianto == param.ToIdImpianto && to.ToOId != param.ToOId).Select(to => to.ToOId).ToListAsync();

                operatoriASupporto = await _context.Operatori.Where(o => es.Contains(o.OId)).ToListAsync();

                return operatoriASupporto.ToArray();
            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, MethodBase.GetCurrentMethod().Name);
                return null;
            }
        }

        [HttpPost]
        [ActionName("UpdateShift")]
        public async Task<RetCode> UpdateShift([FromBody] TurnoOperatori param)
        {
            RetCode rc = new RetCode((int)Globals.RetCodes.Ok, "");
            DateTime actStart, actEnd;
            short actTurn;

            try
            {
                Operatori op = await _context.Operatori.Where(o => o.OId == param.ToOId).FirstOrDefaultAsync();
                (actStart, actEnd, actTurn) = Utilities.CalculateTurn(op.ODtId.Value);

                TurnoOperatori es = await _context.TurnoOperatori.Where(to => to.ToOId.Value == op.OId && (to.ToStartDate.Value == actStart && to.ToEndDate.Value == actEnd) && to.ToTurnNumber == actTurn).FirstOrDefaultAsync();

                if (es != null)
                {
                    es.ToIdImpianto = param.ToIdImpianto;
                    _ = _context.TurnoOperatori.Update(es);
                    int i = await _context.SaveChangesAsync();
                }
                else
                    rc = new RetCode((int)Globals.RetCodes.NotOk, "Errore nel recupero turno operatore");

            }
            catch (Exception ex)
            {
                Globals.g_Logger.Error(ex, "UpdateMateria");
                rc.retCode = (int)Globals.RetCodes.NotOk;
            }

            return rc;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
