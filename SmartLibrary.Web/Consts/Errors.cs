namespace SmartLibrary.Web.Consts
{
    public class Errors
    {
        public const string MaxLength = "Length Can't be more than {1} Characters";
        public const string MaxMinLength = "The {0} must be at least {2} and at max {1} characters long.";
        public const string Duplicated = "{0} With the Same Name is already exists!";
        public const string DuplicatedBook = "Book with the same Title is already exists with the same author!";
        public const string NotAllowedExtensions = "Only .jpg, .jpeg, .png";
        public const string MaxSize = "File Can't Be more than 2MB";
        public const string NotAllowFuture = "Date Can't be in the future";
        public const string InvalidRange = "{0} Should Be Between {1} And {2}";
        public const string ConfirmPasswordNotMatch = "The password and confirmation password do not match.";

    }
}
