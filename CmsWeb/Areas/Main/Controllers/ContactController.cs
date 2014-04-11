using System.Text;
using System.Web.Mvc;
using CmsData;
using CmsWeb.Models.ContactPage;
using UtilityExtensions;

namespace CmsWeb.Areas.Main.Controllers
{
    [RouteArea("Main", AreaPrefix = "")]
    public class ContactController : CmsStaffController
    {
        [HttpGet, Route("Contact/{id}")]
        public ActionResult Index(int id)
        {
            if (ViewExtensions2.UseNewLook())
                return Redirect("/Contact2/" + id);
            var m = new ContactModel(id);
            if (m.contact == null)
                return Content("contact does not exist");

            var edit = (bool?)TempData["ContactEdit"] == true;
            ViewBag.edit = edit;
            return View(m);
        }

        [HttpPost, Route("Contact/RemoveContactee/{cid:int}/{pid:int}")]
        public ActionResult RemoveContactee(int cid, int pid)
        {
            var m = new ContacteesModel(cid);
            m.RemoveContactee(pid);
            return Content("ok");

        }
        [HttpPost, Route("Contact/RemoveContactor/{cid:int}/{pid:int}")]
        public ActionResult RemoveContactor(int cid, int pid)
        {
            var m = new ContactorsModel(cid);
            m.RemoveContactor(pid);
            return Content("ok");
        }

        [HttpPost, Route("Contact/Contactees/{cid:int}")]
        public ActionResult Contactees(int cid)
        {
            var m = new ContacteesModel(cid);
            return View(m);
        }
        [HttpPost, Route("Contact/Contactors/{cid:int}")]
        public ActionResult Contactors(int cid)
        {
            var m = new ContactorsModel(cid);
            return View(m);
        }
        [HttpPost, Route("Contact/ContactEdit/{cid:int}")]
        public ActionResult ContactEdit(int cid)
        {
            var m = new ContactModel(cid);
            if (!m.CanViewComments)
                return View("ContactDisplay", m);
            return View(m);
        }
        [HttpPost, Route("Contact/ContactDisplay/{cid:int}")]
        public ActionResult ContactDisplay(int cid)
        {
            var m = new ContactModel(cid);
            return View(m);
        }
        [HttpPost, Route("Contact/ContactUpdate/{cid:int}")]
        public ActionResult ContactUpdate(int cid, ContactModel c)
        {
            if (!ModelState.IsValid)
                return View("ContactEdit", c);
            c.UpdateContact();
            return View("ContactDisplay", c);
        }
        [HttpPost, Route("Contact/ContactDelete/{cid:int}")]
        public ActionResult ContactDelete(int cid)
        {
            ContactModel.DeleteContact(cid);
            return Redirect("/ContactSearch");
        }
        [HttpPost, Route("Contact/NewTeamContact/{cid:int}")]
        public ActionResult NewTeamContact(int cid)
        {
            var m = new ContactModel(cid);
            var nid = m.AddNewTeamContact();
            return Redirect("/Contact/" + nid);
        }
        [HttpPost, Route("Contact/AddTask/{cid:int}/{pid:int}")]
        public ActionResult AddTask(int cid, int pid)
        {
            var m = new ContacteesModel(cid);
            var tid = m.AddTask(pid);
            return Redirect("/Task/List/" + tid);
        }
    }
}
