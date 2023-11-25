namespace Schedule_Mate.Models
{
    public class PasswordHasher
    {
        public string DecryptPassword(string encryptedPassword)
        {
            byte[] b;
            string decryptedPassword;
            try
            {
                b = Convert.FromBase64String(encryptedPassword);
                decryptedPassword = System.Text.Encoding.ASCII.GetString(b);
            }
            catch (FormatException fe)
            {
                decryptedPassword = "";
            }
            return decryptedPassword;
        }

        public string EncryptPassword(string password)
        {
            byte[] b = System.Text.Encoding.ASCII.GetBytes(password);
            string encryptedPassword = Convert.ToBase64String(b);
            return encryptedPassword;
        }
    }
}