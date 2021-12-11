﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BranSystems.MQTT.Device.IOController
{
    //https://stackoverflow.com/questions/58373915/serialize-objects-implementing-interface-with-system-text-json
    public class TypeMappingConverter<TType, TImplementation> : JsonConverter<TType> where TImplementation : TType
    {
        [return: MaybeNull]
        public override TType Read(
          ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            JsonSerializer.Deserialize<TImplementation>(ref reader, options);

        public override void Write(
          Utf8JsonWriter writer, TType value, JsonSerializerOptions options) =>
            JsonSerializer.Serialize(writer, (TImplementation)value!, options);
    }
}
