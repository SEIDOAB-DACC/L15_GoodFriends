using System;
using Models;
using Models.DTO;

namespace Services
{
    public interface IFriendsService
	{
        //Full set of async methods
        public Task<gstusrInfoAllDto> InfoAsync { get; }
        public Task<gstusrInfoAllDto> SeedAsync(loginUserSessionDto usr, int nrOfItems);
        public Task<gstusrInfoAllDto> RemoveSeedAsync(loginUserSessionDto usr, bool seeded);

        public Task<csRespPageDto<IFriend>> ReadFriendsAsync(loginUserSessionDto usr, bool seeded, bool flat, string filter, int pageNumber, int pageSize);
        public Task<IFriend> ReadFriendAsync(loginUserSessionDto usr, Guid id, bool flat);
        public Task<IFriend> DeleteFriendAsync(loginUserSessionDto usr, Guid id);
        public Task<IFriend> UpdateFriendAsync(loginUserSessionDto usr, csFriendCUdto item);
        public Task<IFriend> CreateFriendAsync(loginUserSessionDto usr, csFriendCUdto item);

        public Task<csRespPageDto<IAddress>> ReadAddressesAsync(loginUserSessionDto usr, bool seeded, bool flat, string filter, int pageNumber, int pageSize);
        public Task<IAddress> ReadAddressAsync(loginUserSessionDto usr, Guid id, bool flat);
        public Task<IAddress> DeleteAddressAsync(loginUserSessionDto usr, Guid id);
        public Task<IAddress> UpdateAddressAsync(loginUserSessionDto usr, csAddressCUdto item);
        public Task<IAddress> CreateAddressAsync(loginUserSessionDto usr, csAddressCUdto item);

        public Task<csRespPageDto<IQuote>> ReadQuotesAsync(loginUserSessionDto usr, bool seeded, bool flat, string filter, int pageNumber, int pageSize);
        public Task<IQuote> ReadQuoteAsync(loginUserSessionDto usr, Guid id, bool flat);
        public Task<IQuote> DeleteQuoteAsync(loginUserSessionDto usr, Guid id);
        public Task<IQuote> UpdateQuoteAsync(loginUserSessionDto usr, csQuoteCUdto item);
        public Task<IQuote> CreateQuoteAsync(loginUserSessionDto usr, csQuoteCUdto item);

        public Task<csRespPageDto<IPet>> ReadPetsAsync(loginUserSessionDto usr, bool seeded, bool flat, string filter, int pageNumber, int pageSize);
        public Task<IPet> ReadPetAsync(loginUserSessionDto usr, Guid id, bool flat);
        public Task<IPet> DeletePetAsync(loginUserSessionDto usr, Guid id);
        public Task<IPet> UpdatePetAsync(loginUserSessionDto usr, csPetCUdto item);
        public Task<IPet> CreatePetAsync(loginUserSessionDto usr, csPetCUdto item);
    }
}

