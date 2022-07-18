using aMongoLibrary;
using mark.webApi.AuthFolder.Models;
using mark.webApi.Models;

namespace mark.webApi.Services;

public class AccountService
{
    private readonly MongoRepo<UserModel> _mongoService;
    public AccountService(MongoRepo<UserModel> mongoService) { _mongoService = mongoService; }

    public async Task<bool> Register(RegisterRequest model, string origin)
    {
        string message = ""; 
        bool isValid = false;
        // validation
        if (await _mongoService.FindOneAsync(x => x.Email == model.Email) != null)
        { message = "E-mail is reeds in gebruik"; isValid = false; }
        else if (await _mongoService.FindOneAsync(x => x.Nickname == model.Nickname) != null)
        { message = "Nickname is reeds in gebruik"; isValid = false; }
        // map register request naar usermodel
        UserModel newUser = model.MyMapper();
        // newUser naar database zonder e-mail verificatie
        await _mongoService.InsertOneAsync(newUser);
        message = "Confirmatie e-mail verzonden!";
        isValid = true;
        return isValid;
    }

    public async Task<UserModel> Login(LoginRequest model)
    {
        var account = await _mongoService.FindOneAsync(x => x.Email == model.Email);
        // validation
        if (account == null || model.Password != account.Password)
        throw new Exception("E-mail of password is fout");
        return account;
    }


}
