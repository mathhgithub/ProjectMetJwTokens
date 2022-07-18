using aMongoLibrary;
using System.ComponentModel.DataAnnotations;

namespace mark.webApi.Models;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    
    public string Nickname { get; set; }

    [Required]
    public string Password { get; set; }

    [Required]
    [Compare("Password", ErrorMessage = "geen match password")]
    public string ConfirmPassword { get; set; }

    
    public bool AcceptTerms { get; set; }


    //helper methods

    public UserModel MyMapper()
    {
        UserModel model = new UserModel();
        model.Nickname = Nickname;
        model.Email = Email;
        model.Password = Password;
        model.BillingInfo = "hier komt billinginfo";
        model.UserType = UserType.subscriber;
        model.AccCreateDay = DateTime.Now;
        return model;
    }
}


