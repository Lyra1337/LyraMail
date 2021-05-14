using System;
using System.Linq;
using AutoMapper;
using Lyralabs.TempMailServer.Data;
using MimeKit;

namespace Lyralabs.TempMailServer
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
            cfg.CreateMap<MimeMessage, MailModel>()
                .ForMember(x => x.Mailbox, opt => opt.Ignore())
                .ForMember(x => x.MailboxId, opt => opt.Ignore())
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.Secret, opt => opt.MapFrom(x => Guid.NewGuid()))
                .ForMember(x => x.Password, opt => opt.Ignore())
                .ForMember(x => x.ReceivedDate, opt => opt.MapFrom(x => x.Date.LocalDateTime))
                .ForMember(x => x.BodyHtml, opt => opt.MapFrom(x => x.HtmlBody))
                .ForMember(x => x.BodyText, opt => opt.MapFrom(x => x.TextBody))
                .ForMember(x => x.FromAddress, opt => opt.MapFrom(x => x.From.OfType<MailboxAddress>().Single().Address))
                .ForMember(x => x.FromName, opt => opt.MapFrom(x => x.From.OfType<MailboxAddress>().Single().Name));
        }
    }
}
