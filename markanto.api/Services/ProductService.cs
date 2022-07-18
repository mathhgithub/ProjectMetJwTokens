namespace markantoApi.Services;

public class ProductService
{
    private readonly MongoRepo<ProductDAL> _repo;
    public ProductService(MongoRepo<ProductDAL> repo) { _repo = repo; }
    //
    public async Task<IEnumerable<Object>> GetAllAsync()
    {
        var output = await _repo.GetAllAsync();
        return output;
    }




}
