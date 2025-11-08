namespace SmartLibrary.Web.Consts
{
    public class Errors
    {
        public const string RequiredField = "Required Field";
        public const string MaxLength = "Length Can't be more than {1} Characters";
        public const string MaxMinLength = "The {0} must be at least {2} and at max {1} characters long.";
        public const string Duplicated = "Another Record With the Same {0} is already exists!";
        public const string DuplicatedBook = "Book with the same Title is already exists with the same author!";
        public const string NotAllowedExtensions = "Only .jpg, .jpeg, .png";
        public const string MaxSize = "File Can't Be more than 2MB";
        public const string NotAllowFuture = "Date Can't be in the future";
        public const string InvalidRange = "{0} Should Be Between {1} And {2}";
        public const string ConfirmPasswordNotMatch = "The password and confirmation password do not match.";
        public const string WeakPassword = "Passwords contain an uppercase character, lowercase character, a digit, and a non-alphanumeric character. Passwords must be at least 8 characters long.";
        public const string InvalidUserName = "Username Can Only Contain letters or Digits.";
        public const string OnlyEnglishLetters = "Only English letters are allowed.";
        public const string OnlyArabicLetters = "Only Arabic letters are allowed.";
        public const string OnlyNumbersAndLetters = "Only Arabic/English letters or digits are allowed.";
        public const string DenySpecialCharacters = "Special characters are not allowed.";
        public const string InvalidMobileNumber = "Invalid mobile number.";
        public const string InvalidNationalId = "Invalid National ID.";
        public const string EmptyImage = "Please Select an Image.";

    }
}
