//Copyright (c) 2011 Martin Cvengros (r6)
//This file is part of Endgame: Singularity II.

//Endgame: Singularity II is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 2 of the License, or
//(at your option) any later version.

//Endgame: Singularity II is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with Endgame: Singularity II; if not, write to the Free Software
//Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

//This file contains serializable dictionary for XML serializer

// http://www.dacris.com/blog/2010/07/31/c-serializable-dictionary-a-working-example/SerializableDictionary

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

[Serializable()] // ...
public class SerializableDictionary<TKey, TVal> : Dictionary<TKey, TVal>, IXmlSerializable, ISerializable
{
	#region Constants
	private const string DictionaryNodeName = "Dictionary";
	private const string ItemNodeName = "Item";
	private const string KeyNodeName = "Key";
	private const string ValueNodeName = "Value";
	#endregion
	#region Constructors
	public SerializableDictionary ()
	{
	}

	public SerializableDictionary (IDictionary<TKey, TVal> dictionary) : base(dictionary)
	{
	}

	public SerializableDictionary (IEqualityComparer<TKey> comparer) : base(comparer)
	{
	}

	public SerializableDictionary (int capacity) : base(capacity)
	{
	}

	public SerializableDictionary (IDictionary<TKey, TVal> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer)
	{
	}

	public SerializableDictionary (int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer)
	{
	}

	#endregion
	#region ISerializable Members

	protected SerializableDictionary (SerializationInfo info, StreamingContext context)
	{
		int itemCount = info.GetInt32 ("ItemCount");
		for (int i = 0; i < itemCount; i++) {
			KeyValuePair<TKey, TVal> kvp = (KeyValuePair<TKey, TVal>)info.GetValue (String.Format ("Item{0}", i), typeof(KeyValuePair<TKey, TVal>));
			this.Add (kvp.Key, kvp.Value);
		}
	}

	void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
	{
		info.AddValue ("ItemCount", this.Count);
		int itemIdx = 0;
		foreach (KeyValuePair<TKey, TVal> kvp in this) {
			info.AddValue (String.Format ("Item{0}", itemIdx), kvp, typeof(KeyValuePair<TKey, TVal>));
			itemIdx++;
		}
	}

	#endregion
	#region IXmlSerializable Members

	void IXmlSerializable.WriteXml (System.Xml.XmlWriter writer)
	{
		//writer.WriteStartElement(DictionaryNodeName);
		foreach (KeyValuePair<TKey, TVal> kvp in this) {
			writer.WriteStartElement (ItemNodeName);
			writer.WriteStartElement (KeyNodeName);
			KeySerializer.Serialize (writer, kvp.Key);
			writer.WriteEndElement ();
			writer.WriteStartElement (ValueNodeName);
//			if ( kvp.Value is float )
//				ValueSerializer.Serialize (writer, string.Format (System.Globalization.CultureInfo.InvariantCulture.NumberFormat, "{0}", kvp.Value) );
//			else
			ValueSerializer.Serialize (writer, kvp.Value );
			writer.WriteEndElement ();
			writer.WriteEndElement ();
		}
		//writer.WriteEndElement();
	}

	void IXmlSerializable.ReadXml (System.Xml.XmlReader reader)
	{
		if (reader.IsEmptyElement) {
			return;
		}
		
		// Move past container
		if (!reader.Read ()) {
			throw new XmlException ("Error in Deserialization of Dictionary");
		}
		
		//reader.ReadStartElement(DictionaryNodeName);
		while (reader.NodeType != XmlNodeType.EndElement) {
			reader.ReadStartElement (ItemNodeName);
			reader.ReadStartElement (KeyNodeName);
			TKey key = (TKey)KeySerializer.Deserialize (reader);
			reader.ReadEndElement ();
			reader.ReadStartElement (ValueNodeName);
			TVal value = (TVal)ValueSerializer.Deserialize (reader);
			reader.ReadEndElement ();
			reader.ReadEndElement ();
			this.Add (key, value);
			reader.MoveToContent ();
		}
		//reader.ReadEndElement();
		
		reader.ReadEndElement ();
		// Read End Element to close Read of containing node
	}

	System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema ()
	{
		return null;
	}

	#endregion
	#region Private Properties
	protected XmlSerializer ValueSerializer {
		get {
			if (valueSerializer == null) {
				valueSerializer = new XmlSerializer (typeof(TVal));
			}
			return valueSerializer;
		}
	}

	private XmlSerializer KeySerializer {
		get {
			if (keySerializer == null) {
				keySerializer = new XmlSerializer (typeof(TKey));
			}
			return keySerializer;
		}
	}
	#endregion
	#region Private Members
	private XmlSerializer keySerializer = null;
	private XmlSerializer valueSerializer = null;
	#endregion
}
