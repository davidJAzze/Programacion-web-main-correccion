namespace UPIICafeWeb.Models
{
    public class ProductoModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal PrecioOriginal { get; set; }
        public decimal Descuento { get; set; } 
        public string ImagenUrl { get; set; }
        
        // Esta es la nueva propiedad que hará funcionar los botones de categorías:
        public int IdCategoria { get; set; } 

        public decimal PrecioFinal => PrecioOriginal - (PrecioOriginal * Descuento);
    }
}