using System.Drawing.Text;

namespace PasswordManager.UI
{

    public static class FontLoader
    {
        private static PrivateFontCollection _fontCollection = new PrivateFontCollection();
        private static FontFamily _pacificoFamily;

        public static Font GetPacificoFont(float size, FontStyle style = FontStyle.Regular)
        {
            if (_pacificoFamily == null)
            {
                string fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Fonts",
                    "Pacifico-Regular.ttf");
                _fontCollection.AddFontFile(fontPath);
                _pacificoFamily = _fontCollection.Families[0];
            }

            return new Font(_pacificoFamily, size, style);
        }
    }

}