namespace SilverSoft.Utils
{
    public class Page
    {
        public bool IsPostBack => false;

        protected virtual void OnInit(EventArgs e)
        {

        }

        public event EventHandler Load;
    }
}
