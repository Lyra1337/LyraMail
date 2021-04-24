using AutoMapper;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                .ForMember(x => x.Message, opt => opt.MapFrom(x => this.ReadBody(x.Body)))
                .ForMember(x => x.FromAddress, opt => opt.MapFrom(x => x.From.OfType<MailboxAddress>().Single().Address))
                .ForMember(x => x.FromName, opt => opt.MapFrom(x => x.From.OfType<MailboxAddress>().Single().Name))
                .ForMember(x => x.To, opt => opt.MapFrom(x => x.To.OfType<MailboxAddress>()));
            
            cfg.CreateMap<MailboxAddress, MailboxDto>();
                //.ForMember(x => x.Address, opt => opt.MapFrom(x => x.Address))
        }

        private string ReadBody(MimeEntity body)
        {
            if (body is TextPart textPart)
            {
                switch (textPart.ContentType.MimeType.ToLower())
                {
                    case "text/plain":
                        return textPart.Text;
                    default:
                        {
                            using var stream = new MemoryStream();
                            textPart.WriteTo(stream);
                            stream.Seek(0, SeekOrigin.Begin);
                            using var reader = new StreamReader(stream, textPart.ContentType.CharsetEncoding);
                            return reader.ReadToEnd();
                        }
                }
            }
            else
            {
                throw new ArgumentException($"{body.GetType().FullName} is not supported");
            }
        }
    }
}
