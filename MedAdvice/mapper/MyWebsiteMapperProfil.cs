using AutoMapper;
using MedAdvice.Models;
using MedAdvice.viewmodel;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedAdvice.mapper
{
    public class MyWebsiteMapperProfil:Profile
    {
        byte[] DownloadImage(IFormFile img)
        {
            if (img == null)
            {
                return new byte[] { 0 };
            }
            else
            {
                byte[] b = new byte[img.Length];
                img.OpenReadStream().Read(b, 0, b.Length);
                return b;
            }

        }
       
        public MyWebsiteMapperProfil()
        {
            CreateMap<ProductImageViewModel, ProductImage>().
               ForMember(x => x.img, y => y.MapFrom(z => DownloadImage(z.img)));
            CreateMap<ProductViewModel, Product>();


            CreateMap<AdviceImageViewModel, AdviceImage>().
                ForMember(x => x.Adviceimg, y => y.MapFrom(z => DownloadImage(z.AdviceImage)));
            CreateMap<AdviceViewmodel, Advice>().
                ForMember(x => x.AdviceHeaderImage, y => y.MapFrom(z => DownloadImage(z.AdviceHeaderImage)));

            CreateMap<BlogImageViewModel, BlogImage>().ForMember
                (x => x.Blogimg, y => y.MapFrom(z => DownloadImage(z.Blogimg)));
            CreateMap<BlogViewModel, Blog>().
                ForMember(x => x.BlogHeaderImage, y => y.MapFrom(z => DownloadImage(z.BlogHeaderImage)));

            CreateMap<DrImageViewModel, DoctorImage>().ForMember
                (x => x.Doctorimg, y => y.MapFrom(z => DownloadImage(z.Doctorimg)));
            CreateMap<DoctorViewModel, Doctor>().
                ForMember(x => x.DrProfileImage, y => y.MapFrom(z => DownloadImage(z.DrProfileImage)));
        }
    }
}
