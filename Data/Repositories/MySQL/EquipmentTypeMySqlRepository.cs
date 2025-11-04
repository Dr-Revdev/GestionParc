using System.Data;
using ProjetParc.Data.Repositories.Contracts;

namespace ProjetParc.Data.Repositories.MySQL
{
    public sealed class EquipmentTypeMySqlRepository : IEquipmentTypeRepository
    {
        public List<EquipmentTypeDto> GetAll()
        {
            var list = new List<EquipmentTypeDto>();

            using IDbConnection connection = DbFactory.Create();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT id, name FROM equipment_type ORDER BY name;";

            using var reader = command.ExecuteReader();
            while (reader.Read())
                list.Add(new EquipmentTypeDto(reader.GetInt32(0), reader.GetString(1)));

            return list;
        }

        public void Update(EquipmentTypeDto equipmentType)
        {
            using var connection = DbFactory.Create();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE equipment_type SET name = @name WHERE id = @id";
            
            var paramId = command.CreateParameter();
            paramId.ParameterName = "@id";
            paramId.Value = equipmentType.Id;
            command.Parameters.Add(paramId);
            
            var paramName = command.CreateParameter();
            paramName.ParameterName = "@name";
            paramName.Value = equipmentType.Name;
            command.Parameters.Add(paramName);
            
            command.ExecuteNonQuery();
        }

        public int Insert(string name)
        {
            using var connection = DbFactory.Create();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO equipment_type (name) VALUES (@name); SELECT LAST_INSERT_ID();";
            
            var param = command.CreateParameter();
            param.ParameterName = "@name";
            param.Value = name;
            command.Parameters.Add(param);
            
            return Convert.ToInt32(command.ExecuteScalar());
        }

        public void Delete(int id)
        {
            using var connection = DbFactory.Create();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM equipment_type WHERE id = @id";
            
            var param = command.CreateParameter();
            param.ParameterName = "@id";
            param.Value = id;
            command.Parameters.Add(param);
            
            command.ExecuteNonQuery();
        }
    }
}