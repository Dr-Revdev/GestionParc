using System.Text;
using GestiParc.Core.DTOs;
using GestiParc.Core.Services;
using Xunit;

namespace GestiParc.Tests.Unit.Core.Services;

public class CsvExportServiceTests
{
    [Fact]
    public void ExportAgents_WritesHeaderAndEscapesValues()
    {
        // Arrange
        var agents = new List<AgentDto>
        {
            new AgentDto(
                Idrh: "A001",
                Nom: "Dupont",
                Prenom: "Jean",
                Email: "jean.dupont@test.fr",
                EquipeId: 10,
                SiteId: 1,
                Heberge: 1,
                Commentaire: "note;avec;point-virgule et \"guillemets\""
            )
        };

        var equipes = new List<EquipeDto> { new EquipeDto(10, "IT") };
        var sites = new List<SiteDto> { new SiteDto(1, "Paris") };

        var service = new CsvExportService(
            agents: agents,
            equipments: new List<EquipmentDto>(),
            equipmentTypes: new List<EquipmentTypeDto>(),
            equipes: equipes,
            sites: sites
        );

        var filePath = Path.Combine(Path.GetTempPath(), $"agents_{Guid.NewGuid():N}.csv");

        try
        {
            // Act
            service.ExportAgents(filePath);

            // Assert
            var lines = File.ReadAllLines(filePath, Encoding.UTF8);
            Assert.True(lines.Length >= 2);

            // Header : on vérifie la présence des colonnes
            Assert.Contains("IDRH", lines[0]);
            Assert.Contains("Nom", lines[0]);
            Assert.Contains("Prénom", lines[0]);
            Assert.Contains("Équipe", lines[0]);
            Assert.Contains("Site", lines[0]);
            Assert.Contains("Hébergé", lines[0]);

            // Data line: vérifie mapping équipe/site + hébergé Oui + échappement
            Assert.Contains("A001", lines[1]);
            Assert.Contains("IT", lines[1]);
            Assert.Contains("Paris", lines[1]);
            Assert.Contains("Oui", lines[1]);

            // Le commentaire contient ; et " => doit être entre guillemets + "" pour échapper "
            Assert.Contains("\"note;avec;point-virgule et \"\"guillemets\"\"\"", lines[1]);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public void ExportAgents_SortsByNomThenPrenom_AndMapsHeberge()
    {
        // Arrange
        var agents = new List<AgentDto>
        {
            new AgentDto(
                Idrh: "A002",
                Nom: "Dupont",
                Prenom: "Zoe",
                Email: null,
                EquipeId: null,
                SiteId: null,
                Heberge: 0,
                Commentaire: null
            ),
            new AgentDto(
                Idrh: "A001",
                Nom: "Dupont",
                Prenom: "Alice",
                Email: null,
                EquipeId: null,
                SiteId: null,
                Heberge: 1,
                Commentaire: null
            )
        };

        var service = new CsvExportService(
            agents: agents,
            equipments: new List<EquipmentDto>(),
            equipmentTypes: new List<EquipmentTypeDto>(),
            equipes: new List<EquipeDto>(),
            sites: new List<SiteDto>()
        );

        var filePath = Path.Combine(Path.GetTempPath(), $"agents_{Guid.NewGuid():N}.csv");

        try
        {
            // Act
            service.ExportAgents(filePath);

            // Assert
            var lines = File.ReadAllLines(filePath, Encoding.UTF8);
            Assert.True(lines.Length >= 3);

            // Tri: Alice avant Zoe
            Assert.Contains("A001", lines[1]);
            Assert.Contains("A002", lines[2]);

            // Mapping heberge: 1 => Oui, 0 => Non
            Assert.Contains("Oui", lines[1]);
            Assert.Contains("Non", lines[2]);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public void ExportEquipments_WritesMappedTypeAgentAndEtatPret()
    {
        // Arrange
        var agents = new List<AgentDto>
        {
            new AgentDto(
                Idrh: "A001",
                Nom: "Dupont",
                Prenom: "Jean",
                Email: "jean.dupont@test.fr",
                EquipeId: null,
                SiteId: null,
                Heberge: 0,
                Commentaire: null
            )
        };

        var equipmentTypes = new List<EquipmentTypeDto>
        {
            new EquipmentTypeDto(
                Id: 1,
                Name: "PC PORTABLE"
            )
        };

        var equipments = new List<EquipmentDto>
        {
            new EquipmentDto(
                IdEquipement: "EQ-1",
                TypeId: 1,
                Nom: "Dell Latitude",
                CodeParc: "P-001",
                NumeroSerie: "SN-123",
                Marque: "Dell",
                Commentaire: null,
                EtatPret: 1,
                Idrh: "A001",
                DateRenduDsem: null
            )
        };

        var service = new CsvExportService(
            agents: agents,
            equipments: equipments,
            equipmentTypes: equipmentTypes,
            equipes: new List<EquipeDto>(),
            sites: new List<SiteDto>()
        );

        var filePath = Path.Combine(Path.GetTempPath(), $"equipements_{Guid.NewGuid():N}.csv");

        try
        {
            // Act
            service.ExportEquipments(filePath);

            // Assert
            var lines = File.ReadAllLines(filePath, Encoding.UTF8);
            Assert.True(lines.Length >= 2);

            var header = lines[0];
            var row = lines[1];

            Assert.Contains("Type", header);
            Assert.Contains("État", header);
            Assert.Contains("Agent", header);

            Assert.Contains("PC PORTABLE", row);
            Assert.Contains("Dupont Jean", row);
            Assert.Contains("En prêt", row);
            Assert.Contains("Dell Latitude", row);

        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Theory]
    [InlineData(0, "Disponible")]
    [InlineData(1, "En prêt")]
    [InlineData(2, "Rendu DSEM")]
    [InlineData(999, "Inconnu")]
    public void ExportEquipments_WritesCorrectEtatPretLabel(int etatPret, string expectedLabel)
    {
        // Arrange
        var agents = new List<AgentDto>
        {
            new AgentDto(
                Idrh: "A001",
                Nom: "Dupont",
                Prenom: "Jean",
                Email: "jean.dupont@test.fr",
                EquipeId: null,
                SiteId: null,
                Heberge: 0,
                Commentaire: null
            )
        };

        var equipmentTypes = new List<EquipmentTypeDto>
        {
            new EquipmentTypeDto(
                Id: 1,
                Name: "PC PORTABLE"
            )
        };

        var equipments = new List<EquipmentDto>
        {
            new EquipmentDto(
                IdEquipement: "EQ-1",
                TypeId: 1,
                Nom: "Dell Latitude",
                CodeParc: "P-001",
                NumeroSerie: "SN-123",
                Marque: "Dell",
                Commentaire: null,
                EtatPret: etatPret,
                Idrh: "A001",
                DateRenduDsem: null
            )
        };

        var service = new CsvExportService(
            agents: agents,
            equipments: equipments,
            equipmentTypes: equipmentTypes,
            equipes: new List<EquipeDto>(),
            sites: new List<SiteDto>()
        );

        var filePath = Path.Combine(Path.GetTempPath(), $"equipements_{Guid.NewGuid():N}.csv");

        try
        {
            // Act
            service.ExportEquipments(filePath);

            // Assert
            var lines = File.ReadAllLines(filePath, Encoding.UTF8);
            Assert.True(lines.Length >= 2);

            var row = lines[1];

            Assert.Contains(expectedLabel, row);

        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public void ExportLoans_WritesUnionHeaders_WhenSomeRowsHaveMoreEquipmentColumns()
    {
        // Arrange
        var agents = new List<AgentDto>
        {
            new AgentDto(
                Idrh: "A001",
                Nom: "Alpha",
                Prenom: "Anne",
                Email: "anne.alpha@test.fr",
                EquipeId: 1,
                SiteId: null,
                Heberge: 0,
                Commentaire: null
            ),
            new AgentDto(
                Idrh: "A002",
                Nom: "Beta",
                Prenom: "Bob",
                Email: "bob.beta@test.fr",
                EquipeId: 1,
                SiteId: null,
                Heberge: 0,
                Commentaire: null
            )
        };

        var equipes = new List<EquipeDto> { new EquipeDto(1, "IT") };
        var types = new List<EquipmentTypeDto> { new EquipmentTypeDto(1, "PC") };

        // A001 : 1 equipement en pret, A002 : 2 equipements en pret
        var equipments = new List<EquipmentDto>
        {
            new EquipmentDto(
                IdEquipement: "EQ-1",
                TypeId: 1,
                Nom: "Laptop A",
                CodeParc: "P-001\nX",
                NumeroSerie: null,
                Marque: null,
                Commentaire: "ligne1\nligne2",
                EtatPret: 1,
                Idrh: "A001",
                DateRenduDsem: null
            ),
            new EquipmentDto(
                IdEquipement: "EQ-2",
                TypeId: 1,
                Nom: "Laptop B",
                CodeParc: "P-002",
                NumeroSerie: null,
                Marque: null,
                Commentaire: null,
                EtatPret: 1,
                Idrh: "A002",
                DateRenduDsem: null
            ),
            new EquipmentDto(
                IdEquipement: "EQ-3",
                TypeId: 1,
                Nom: "Laptop C",
                CodeParc: "P-003",
                NumeroSerie: null,
                Marque: null,
                Commentaire: null,
                EtatPret: 1,
                Idrh: "A002",
                DateRenduDsem: null
            )
        };

        var service = new CsvExportService(
            agents: agents,
            equipments: equipments,
            equipmentTypes: types,
            equipes: equipes,
            sites: new List<SiteDto>()
        );

        var filePath = Path.Combine(Path.GetTempPath(), $"prets_{Guid.NewGuid():N}.csv");

        try
        {
            // Act
            service.ExportLoans(filePath);

            // Assert
            var lines = File.ReadAllLines(filePath, Encoding.UTF8);
            Assert.True(lines.Length >= 3);

            var header = lines[0];

            // Union: le fichier doit contenir la colonne "Équipement 2" car un agent en a deux
            Assert.Contains("Équipement 2", header);

            // Echappement retours ligne : la valeur exportee contient un retour-ligne => doit etre entre guillemets
            Assert.Contains("\"PC - P-001\nX\"", string.Join("\n", lines));
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}