namespace EC2.ChatEngine
{
	/// <summary>
	/// Klasa reprezentująca plik
	/// </summary>
	public class HashFile
	{
		#region Properties

		/// <summary>
		/// Nazwa pliku
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Typ pliku
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// Zawartość pliku
		/// </summary>
		public string Content { get; set; }

		#endregion Properties

		#region Constructors

		/// <summary>
		/// Konstruktor Klasy
		/// </summary>
		/// <param name="Stream">Zawartość pliku</param>
		/// <param name="Name">Nazwa pliku</param>
		/// <param name="Type">Typ pliku</param>
		public HashFile(string Stream, string Name, string Type)
		{
			this.Name = Name;
			this.Content = Stream;
			this.Type = Type;
		}

		#endregion Constructors
	}
}