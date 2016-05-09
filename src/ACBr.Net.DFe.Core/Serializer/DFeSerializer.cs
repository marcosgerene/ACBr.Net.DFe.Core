// ***********************************************************************
// Assembly         : ACBr.Net.DFe.Core
// Author           : RFTD
// Created          : 27-03-2016
//
// Last Modified By : RFTD
// Last Modified On : 27-03-2016
// ***********************************************************************
// <copyright file="DFeSerializer.cs" company="ACBr.Net">
//     Copyright (c) ACBr.Net. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ACBr.Net.Core.Exceptions;
using ACBr.Net.Core.Extensions;
using ACBr.Net.DFe.Core.Attributes;
using ACBr.Net.DFe.Core.Extensions;
using ACBr.Net.DFe.Core.Internal;

namespace ACBr.Net.DFe.Core.Serializer
{

	/// <summary>
	/// Class DFeSerializer.
	/// </summary>
	public class DFeSerializer
	{
		#region Fields

		private readonly Type tipoDFe;

		#endregion Fields

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="DFeSerializer{T}"/> class.
		/// </summary>
		internal DFeSerializer(Type tipo)
		{
			Guard.Against<ArgumentException>(tipo.IsGenericType, "N�o � possivel serializar uma classe generica !");
			Guard.Against<ArgumentException>(!tipo.HasAttribute<DFeRootAttribute>(), "N�o � uma classe DFe !");
			tipoDFe = tipo;
			Options = new SerializerOptions();
		}

		#endregion Constructors

		#region Propriedades

		/// <summary>
		/// Gets the options.
		/// </summary>
		/// <value>The options.</value>
		public SerializerOptions Options { get; }

		#endregion Propriedades

		#region Methods

		#region Create

		/// <summary>
		/// Creates the serializer.
		/// </summary>
		/// <param name="tipo">The tipo.</param>
		/// <returns>ACBr.Net.DFe.Core.Serializer.DFeSerializer.</returns>
		public static DFeSerializer CreateSerializer(Type tipo)
		{
			return new DFeSerializer(tipo);
		}

		/// <summary>
		/// Creates the serializer.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns>DFeSerializer.</returns>
		public static DFeSerializer<T> CreateSerializer<T>() where T : class 
		{
			return new DFeSerializer<T>();
		}

		#endregion Create

		#region Serialize

		/// <summary>
		/// Serializes the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="path">The path.</param>
		public bool Serialize(object item, string path)
		{
			Guard.Against<ArgumentException>(item.GetType() != tipoDFe, "Tipo diferente do informado");

			Options.ErrosAlertas.Clear();
			if (item.IsNull())
			{
				Options.ErrosAlertas.Add("O item � nulo !");
				return false;
			}

			var xmldoc = Serialize(item);
			var ret = !Options.ErrosAlertas.Any();
			if(Options.IdentarXml)
				xmldoc.Save(path);
			else
				xmldoc.Save(path, SaveOptions.DisableFormatting);
			return ret;
		}

		/// <summary>
		/// Serializes the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="stream">The stream.</param>
		/// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
		public bool Serialize(object item, Stream stream)
		{
			Guard.Against<ArgumentException>(item.GetType() != tipoDFe, "Tipo diferente do informado");

			Options.ErrosAlertas.Clear();
			if (item.IsNull())
			{
				Options.ErrosAlertas.Add("O item � nulo !");
				return false;
			}

			var xmldoc = Serialize(item);
			var ret = !Options.ErrosAlertas.Any();
			if (Options.IdentarXml)
				xmldoc.Save(stream);
			else
				xmldoc.Save(stream, SaveOptions.DisableFormatting);

			stream.Position = 0;
			return ret;
		}

		private XDocument Serialize(object item)
		{
			var xmldoc = new XDocument
			{
				Declaration = new XDeclaration("1.0", "UTF-8", null)
			};

			var rooTag = item.GetType().GetAttribute<DFeRootAttribute>();
			var rootName = rooTag != null && !rooTag.Name.IsEmpty()
				? rooTag.Name : tipoDFe.Name;

			var rootElement = ObjectSerializer.Serialize(item, rootName, Options);
			xmldoc.Add(rootElement);
			xmldoc.RemoveEmptyNamespace();
			return xmldoc;
		}
		
		#endregion Serialize

		#region Deserialize

		/// <summary>
		/// Deserializes the specified path.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns>System.Object.</returns>
		public object Deserialize(string path)
		{
			var xmlDoc = XDocument.Load(path);
			return Deserialize(xmlDoc);
		}

		/// <summary>
		/// Deserializes the specified stream.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <returns>System.Object.</returns>
		public object Deserialize(Stream stream)
		{
			var xmlDoc = XDocument.Load(stream);
			return Deserialize(xmlDoc);
		}

		private object Deserialize(XDocument xmlDoc)
		{
			var rooTag = tipoDFe.GetAttribute<DFeRootAttribute>();
			var rootName = rooTag != null && !rooTag.Name.IsEmpty()
				? rooTag.Name : tipoDFe.Name;

			return rootName != xmlDoc.Root?.Name ? null : 
				   ObjectSerializer.Deserialize(tipoDFe, xmlDoc.Root, Options);
		}

		#endregion Deserialize
		
		#endregion Methods
	}

	/// <summary>
	/// Class DFeSerializer. This class cannot be inherited.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso>
	///     <cref>ACBr.Net.DFe.Core.Serializer.DFeSerializerBase</cref>
	/// </seealso>
	public sealed class DFeSerializer<T> : DFeSerializer where T : class
	{
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="DFeSerializer{T}"/> class.
		/// </summary>
		internal DFeSerializer() : base(typeof(T))
		{
		}

		#endregion Constructors

		#region Methods

		/// <summary>
		/// Serializes the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="path">The path.</param>
		public bool Serialize(T item, string path)
		{
			return base.Serialize(item, path);
		}

		/// <summary>
		/// Serializes the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="stream">The stream.</param>
		/// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
		public bool Serialize(T item, Stream stream)
		{
			return base.Serialize(item, stream);
		}

		/// <summary>
		/// Deserializes the specified path.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns>T.</returns>
		public new T Deserialize(string path)
		{
			return (T)base.Deserialize(path);
		}

		/// <summary>
		/// Deserializes the specified path.
		/// </summary>
		/// <param name="stream">The path.</param>
		/// <returns>T.</returns>
		public new T Deserialize(Stream stream)
		{
			return (T)base.Deserialize(stream);
		}

		#endregion Methods
	}
}