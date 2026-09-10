using MedAdvice.Models;
using MedAdvice.viewmodel;

namespace MedAdvice.mapper
{
    /// Converts view models to entities by explicit assignment.
    /// Image bytes are deliberately not handled here: uploads need validation and error
    /// reporting, so callers read them through MedAdvice.Services.ImageUpload and assign
    /// the result. Identifiers are not copied either; the database generates them.
    public static class EntityMapping
    {
        public static Product ToEntity(this ProductViewModel model)
        {
            return new Product
            {
                Date = model.Date,
                englishname = model.englishname,
                color = model.color,
                weight = model.weight,
                size = model.size,
                productmodel = model.productmodel,
                price = model.price,
                count = model.count,
                discount = model.discount,
                descreption = model.descreption,
                BrandId = model.BrandId,
                ProductCategoryId = model.ProductCategoryId,
            };
        }

        public static ProductImage ToEntity(this ProductImageViewModel model)
        {
            return new ProductImage
            {
                title = model.title,
            };
        }

        public static Blog ToEntity(this BlogViewModel model)
        {
            return new Blog
            {
                BlogTitle = model.BlogTitle,
                BlogDate = model.BlogDate,
                BlogText = model.BlogText,
                BlogBriefText = model.BlogBriefText,
                BlogCategoryId = model.BlogCategoryId,
            };
        }

        public static BlogImage ToEntity(this BlogImageViewModel model)
        {
            return new BlogImage
            {
                BlogImageTitle = model.BlogImageTitle,
            };
        }

        public static Doctor ToEntity(this DoctorViewModel model)
        {
            return new Doctor
            {
                MedicalCouncilNo = model.MedicalCouncilNo,
                DrBirthDate = model.DrBirthDate,
                FirstName = model.FirstName,
                FamillyName = model.FamillyName,
                DrServiceLocation = model.DrServiceLocation,
                CollaborationDate = model.CollaborationDate,
                NationalCode = model.NationalCode,
                PhoneNumber = model.PhoneNumber,
                Mobilenumber = model.Mobilenumber,
                InstagramId = model.InstagramId,
                TwitterId = model.TwitterId,
                FacebookId = model.FacebookId,
                EmailAdress = model.EmailAdress,
                DrBriefIntroduction = model.DrBriefIntroduction,
                DrDetails = model.DrDetails,
                DrSpacialityId = model.DrSpacialityId,
            };
        }

        public static DoctorImage ToEntity(this DrImageViewModel model)
        {
            return new DoctorImage
            {
                DoctorImageTitle = model.DoctorImageTitle,
            };
        }

        public static AdviceImage ToEntity(this AdviceImageViewModel model)
        {
            return new AdviceImage
            {
                AdviceImageTitle = model.AdviceImageTitle,
            };
        }
    }
}
