namespace HumioAPI.Contracts.Promocodes;

public sealed record PromocodesPageResponse(int Total, PromocodeResponse[] Items);
