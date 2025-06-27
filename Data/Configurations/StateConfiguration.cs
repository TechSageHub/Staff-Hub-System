using Data.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations
{
    public class StateConfiguration : IEntityTypeConfiguration<NigeriaState>
    {
        public void Configure(EntityTypeBuilder<NigeriaState> builder)
        {
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Name).IsRequired();

            // Seeding Nigerian States
            builder.HasData(
            new NigeriaState { Id = 1, Name = "Abia", Code = "AB" },
            new NigeriaState { Id = 2, Name = "Adamawa", Code = "AD" },
            new NigeriaState { Id = 3, Name = "Akwa Ibom", Code = "AK" },
            new NigeriaState { Id = 4, Name = "Anambra", Code = "AN" },
            new NigeriaState { Id = 5, Name = "Bauchi", Code = "BA" },
            new NigeriaState { Id = 6, Name = "Bayelsa", Code = "BY" },
            new NigeriaState { Id = 7, Name = "Benue", Code = "BN" },
            new NigeriaState { Id = 8, Name = "Borno", Code = "BO" },
            new NigeriaState { Id = 9, Name = "Cross River", Code = "CR" },
            new NigeriaState { Id = 10, Name = "Delta", Code = "DE" },
            new NigeriaState { Id = 11, Name = "Ebonyi", Code = "EB" },
            new NigeriaState { Id = 12, Name = "Edo", Code = "ED" },
            new NigeriaState { Id = 13, Name = "Ekiti", Code = "EK" },
            new NigeriaState { Id = 14, Name = "Enugu", Code = "EN" },
            new NigeriaState { Id = 15, Name = "Gombe", Code = "GO" },
            new NigeriaState { Id = 16, Name = "Imo", Code = "IM" },
            new NigeriaState { Id = 17, Name = "Jigawa", Code = "JI" },
            new NigeriaState { Id = 18, Name = "Kaduna", Code = "KD" },
            new NigeriaState { Id = 19, Name = "Kano", Code = "KN" },
            new NigeriaState { Id = 20, Name = "Katsina", Code = "KT" },
            new NigeriaState { Id = 21, Name = "Kebbi", Code = "KE" },
            new NigeriaState { Id = 22, Name = "Kogi", Code = "KO" },
            new NigeriaState { Id = 23, Name = "Kwara", Code = "KW" },
            new NigeriaState { Id = 24, Name = "Lagos", Code = "LA" },
            new NigeriaState { Id = 25, Name = "Nasarawa", Code = "NA" },
            new NigeriaState { Id = 26, Name = "Niger", Code = "NI" },
            new NigeriaState { Id = 27, Name = "Ogun", Code = "OG" },
            new NigeriaState { Id = 28, Name = "Ondo", Code = "ON" },
            new NigeriaState { Id = 29, Name = "Osun", Code = "OS" },
            new NigeriaState { Id = 30, Name = "Oyo", Code = "OY" },
            new NigeriaState { Id = 31, Name = "Plateau", Code = "PL" },
            new NigeriaState { Id = 32, Name = "Rivers", Code = "RI" },
            new NigeriaState { Id = 33, Name = "Sokoto", Code = "SO" },
            new NigeriaState { Id = 34, Name = "Taraba", Code = "TA" },
            new NigeriaState { Id = 35, Name = "Yobe", Code = "YO" },
            new NigeriaState { Id = 36, Name = "Zamfara", Code = "ZA" },
            new NigeriaState { Id = 37, Name = "Federal Capital Territory", Code = "FCT" }
            );
        }
    }
}
