SET NOCOUNT ON;
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'پزشک عمومی و خانواده' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'پزشک عمومی و خانواده', NULL, N'/uploads/advices/specialty-family-medicine.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'قلب و عروق' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'قلب و عروق', NULL, N'/uploads/advices/specialty-cardiology.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'مغز و اعصاب' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'مغز و اعصاب', NULL, N'/uploads/advices/specialty-neurology.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'ارتوپدی و جراحی استخوان' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'ارتوپدی و جراحی استخوان', NULL, N'/uploads/advices/specialty-orthopedics.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'گوارش و کبد' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'گوارش و کبد', NULL, N'/uploads/advices/specialty-gastroenterology.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'پوست، مو و زیبایی' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'پوست، مو و زیبایی', NULL, N'/uploads/advices/specialty-dermatology.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'زنان، زایمان و ناباروری' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'زنان، زایمان و ناباروری', NULL, N'/uploads/advices/specialty-gynecology.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'کودکان و نوزادان' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'کودکان و نوزادان', NULL, N'/uploads/advices/specialty-pediatrics.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'چشم‌پزشکی' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'چشم‌پزشکی', NULL, N'/uploads/advices/specialty-ophthalmology.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'گوش، حلق و بینی' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'گوش، حلق و بینی', NULL, N'/uploads/advices/specialty-ent.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'اورولوژی و مجاری ادراری' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'اورولوژی و مجاری ادراری', NULL, N'/uploads/advices/specialty-urology.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'غدد، دیابت و متابولیسم' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'غدد، دیابت و متابولیسم', NULL, N'/uploads/advices/specialty-endocrinology.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'روانپزشکی' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'روانپزشکی', NULL, N'/uploads/advices/specialty-psychiatry.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'روانشناسی و مشاوره' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'روانشناسی و مشاوره', NULL, N'/uploads/advices/specialty-psychology.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'آلرژی و ایمونولوژی' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'آلرژی و ایمونولوژی', NULL, N'/uploads/advices/specialty-allergy-immunology.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'خون‌شناسی و انکولوژی' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'خون‌شناسی و انکولوژی', NULL, N'/uploads/advices/specialty-hematology-oncology.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'بیماری‌های عفونی' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'بیماری‌های عفونی', NULL, N'/uploads/advices/specialty-infectious-diseases.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'ریه و تنفس' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'ریه و تنفس', NULL, N'/uploads/advices/specialty-pulmonology.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'روماتولوژی (مفاصل)' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'روماتولوژی (مفاصل)', NULL, N'/uploads/advices/specialty-rheumatology.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'جراحی عمومی' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'جراحی عمومی', NULL, N'/uploads/advices/specialty-general-surgery.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'جراحی مغز و اعصاب' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'جراحی مغز و اعصاب', NULL, N'/uploads/advices/specialty-neurosurgery.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'جراحی پلاستیک و ترمیمی' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'جراحی پلاستیک و ترمیمی', NULL, N'/uploads/advices/specialty-plastic-surgery.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'بیهوشی' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'بیهوشی', NULL, N'/uploads/advices/specialty-anesthesiology.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'رادیولوژی و تصویربرداری' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'رادیولوژی و تصویربرداری', NULL, N'/uploads/advices/specialty-radiology.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'پاتولوژی و آزمایشگاه' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'پاتولوژی و آزمایشگاه', NULL, N'/uploads/advices/specialty-pathology.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'طب اورژانس' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'طب اورژانس', NULL, N'/uploads/advices/specialty-emergency-medicine.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'طب فیزیکی و توانبخشی' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'طب فیزیکی و توانبخشی', NULL, N'/uploads/advices/specialty-pmr.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'دندانپزشکی' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'دندانپزشکی', NULL, N'/uploads/advices/specialty-dentistry.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'طب ورزشی' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'طب ورزشی', NULL, N'/uploads/advices/specialty-sports-medicine.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'طب سالمندی' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'طب سالمندی', NULL, N'/uploads/advices/specialty-geriatrics.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'تغذیه و رژیم‌درمانی' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'تغذیه و رژیم‌درمانی', NULL, N'/uploads/advices/specialty-nutrition.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'طب خواب' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'طب خواب', NULL, N'/uploads/advices/specialty-sleep-medicine.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'طب سوزنی' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'طب سوزنی', NULL, N'/uploads/advices/specialty-acupuncture.png');
IF NOT EXISTS (SELECT 1 FROM [adviceCategories] WHERE [AdviceCategoryname] = N'کاتگوت گذاری و کاهش وزن' AND [AdviceCategoryParentId] IS NULL)
    INSERT INTO [adviceCategories] ([AdviceCategoryname], [AdviceCategoryParentId], [AdviceCategoryPicturePath])
    VALUES (N'کاتگوت گذاری و کاهش وزن', NULL, N'/uploads/advices/specialty-catgut-embedding.png');
