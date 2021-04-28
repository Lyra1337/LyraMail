using System.Linq;
using AutoMapper;
using MimeKit;

namespace Lyralabs.Net.TempMailServer
{
    public sealed class MapperService
    {
        public IMapper Mapper { get; }

        public MapperService()
        {
            this.Mapper = this.BuildMapper();
        }

        private IMapper BuildMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                this.MapMail(cfg);
            });

            config.AssertConfigurationIsValid();

            return config.CreateMapper();
        }

        private void MapMail(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<MimeMessage, EmailDto>()
                .ForMember(x => x.ReceivedDate, opt => opt.MapFrom(x => x.Date.LocalDateTime))
                .ForMember(x => x.BodyHtml, opt => opt.MapFrom(x => x.HtmlBody))
                .ForMember(x => x.BodyText, opt => opt.MapFrom(x => x.TextBody))
                .ForMember(x => x.FromAddress, opt => opt.MapFrom(x => x.From.OfType<MailboxAddress>().Single().Address))
                .ForMember(x => x.FromName, opt => opt.MapFrom(x => x.From.OfType<MailboxAddress>().Single().Name))
                .ForMember(x => x.To, opt => opt.MapFrom(x => x.To.OfType<MailboxAddress>()));

            cfg.CreateMap<MailboxAddress, MailboxAddressDto>();
        }
    }
}
