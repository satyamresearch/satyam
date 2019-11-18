// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: tensorflow_serving/apis/regression.proto
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Tensorflow.Serving {

  /// <summary>Holder for reflection information generated from tensorflow_serving/apis/regression.proto</summary>
  public static partial class RegressionReflection {

    #region Descriptor
    /// <summary>File descriptor for tensorflow_serving/apis/regression.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static RegressionReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Cih0ZW5zb3JmbG93X3NlcnZpbmcvYXBpcy9yZWdyZXNzaW9uLnByb3RvEhJ0",
            "ZW5zb3JmbG93LnNlcnZpbmcaI3RlbnNvcmZsb3dfc2VydmluZy9hcGlzL2lu",
            "cHV0LnByb3RvGiN0ZW5zb3JmbG93X3NlcnZpbmcvYXBpcy9tb2RlbC5wcm90",
            "byIbCgpSZWdyZXNzaW9uEg0KBXZhbHVlGAEgASgCIkcKEFJlZ3Jlc3Npb25S",
            "ZXN1bHQSMwoLcmVncmVzc2lvbnMYASADKAsyHi50ZW5zb3JmbG93LnNlcnZp",
            "bmcuUmVncmVzc2lvbiJwChFSZWdyZXNzaW9uUmVxdWVzdBIxCgptb2RlbF9z",
            "cGVjGAEgASgLMh0udGVuc29yZmxvdy5zZXJ2aW5nLk1vZGVsU3BlYxIoCgVp",
            "bnB1dBgCIAEoCzIZLnRlbnNvcmZsb3cuc2VydmluZy5JbnB1dCJ9ChJSZWdy",
            "ZXNzaW9uUmVzcG9uc2USMQoKbW9kZWxfc3BlYxgCIAEoCzIdLnRlbnNvcmZs",
            "b3cuc2VydmluZy5Nb2RlbFNwZWMSNAoGcmVzdWx0GAEgASgLMiQudGVuc29y",
            "Zmxvdy5zZXJ2aW5nLlJlZ3Jlc3Npb25SZXN1bHRCA/gBAWIGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Tensorflow.Serving.InputReflection.Descriptor, global::Tensorflow.Serving.ModelReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Tensorflow.Serving.Regression), global::Tensorflow.Serving.Regression.Parser, new[]{ "Value" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Tensorflow.Serving.RegressionResult), global::Tensorflow.Serving.RegressionResult.Parser, new[]{ "Regressions" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Tensorflow.Serving.RegressionRequest), global::Tensorflow.Serving.RegressionRequest.Parser, new[]{ "ModelSpec", "Input" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Tensorflow.Serving.RegressionResponse), global::Tensorflow.Serving.RegressionResponse.Parser, new[]{ "ModelSpec", "Result" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  /// <summary>
  /// Regression result for a single item (tensorflow.Example).
  /// </summary>
  public sealed partial class Regression : pb::IMessage<Regression> {
    private static readonly pb::MessageParser<Regression> _parser = new pb::MessageParser<Regression>(() => new Regression());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Regression> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Tensorflow.Serving.RegressionReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Regression() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Regression(Regression other) : this() {
      value_ = other.value_;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Regression Clone() {
      return new Regression(this);
    }

    /// <summary>Field number for the "value" field.</summary>
    public const int ValueFieldNumber = 1;
    private float value_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public float Value {
      get { return value_; }
      set {
        value_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Regression);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Regression other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Value != other.Value) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Value != 0F) hash ^= Value.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Value != 0F) {
        output.WriteRawTag(13);
        output.WriteFloat(Value);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Value != 0F) {
        size += 1 + 4;
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Regression other) {
      if (other == null) {
        return;
      }
      if (other.Value != 0F) {
        Value = other.Value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 13: {
            Value = input.ReadFloat();
            break;
          }
        }
      }
    }

  }

  /// <summary>
  /// Contains one result per input example, in the same order as the input in
  /// RegressionRequest.
  /// </summary>
  public sealed partial class RegressionResult : pb::IMessage<RegressionResult> {
    private static readonly pb::MessageParser<RegressionResult> _parser = new pb::MessageParser<RegressionResult>(() => new RegressionResult());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<RegressionResult> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Tensorflow.Serving.RegressionReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public RegressionResult() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public RegressionResult(RegressionResult other) : this() {
      regressions_ = other.regressions_.Clone();
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public RegressionResult Clone() {
      return new RegressionResult(this);
    }

    /// <summary>Field number for the "regressions" field.</summary>
    public const int RegressionsFieldNumber = 1;
    private static readonly pb::FieldCodec<global::Tensorflow.Serving.Regression> _repeated_regressions_codec
        = pb::FieldCodec.ForMessage(10, global::Tensorflow.Serving.Regression.Parser);
    private readonly pbc::RepeatedField<global::Tensorflow.Serving.Regression> regressions_ = new pbc::RepeatedField<global::Tensorflow.Serving.Regression>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::Tensorflow.Serving.Regression> Regressions {
      get { return regressions_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as RegressionResult);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(RegressionResult other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if(!regressions_.Equals(other.regressions_)) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      hash ^= regressions_.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      regressions_.WriteTo(output, _repeated_regressions_codec);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      size += regressions_.CalculateSize(_repeated_regressions_codec);
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(RegressionResult other) {
      if (other == null) {
        return;
      }
      regressions_.Add(other.regressions_);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 10: {
            regressions_.AddEntriesFrom(input, _repeated_regressions_codec);
            break;
          }
        }
      }
    }

  }

  public sealed partial class RegressionRequest : pb::IMessage<RegressionRequest> {
    private static readonly pb::MessageParser<RegressionRequest> _parser = new pb::MessageParser<RegressionRequest>(() => new RegressionRequest());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<RegressionRequest> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Tensorflow.Serving.RegressionReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public RegressionRequest() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public RegressionRequest(RegressionRequest other) : this() {
      ModelSpec = other.modelSpec_ != null ? other.ModelSpec.Clone() : null;
      Input = other.input_ != null ? other.Input.Clone() : null;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public RegressionRequest Clone() {
      return new RegressionRequest(this);
    }

    /// <summary>Field number for the "model_spec" field.</summary>
    public const int ModelSpecFieldNumber = 1;
    private global::Tensorflow.Serving.ModelSpec modelSpec_;
    /// <summary>
    /// Model Specification. If version is not specified, will use the latest
    /// (numerical) version.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Tensorflow.Serving.ModelSpec ModelSpec {
      get { return modelSpec_; }
      set {
        modelSpec_ = value;
      }
    }

    /// <summary>Field number for the "input" field.</summary>
    public const int InputFieldNumber = 2;
    private global::Tensorflow.Serving.Input input_;
    /// <summary>
    /// Input data.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Tensorflow.Serving.Input Input {
      get { return input_; }
      set {
        input_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as RegressionRequest);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(RegressionRequest other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(ModelSpec, other.ModelSpec)) return false;
      if (!object.Equals(Input, other.Input)) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (modelSpec_ != null) hash ^= ModelSpec.GetHashCode();
      if (input_ != null) hash ^= Input.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (modelSpec_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(ModelSpec);
      }
      if (input_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(Input);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (modelSpec_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(ModelSpec);
      }
      if (input_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Input);
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(RegressionRequest other) {
      if (other == null) {
        return;
      }
      if (other.modelSpec_ != null) {
        if (modelSpec_ == null) {
          modelSpec_ = new global::Tensorflow.Serving.ModelSpec();
        }
        ModelSpec.MergeFrom(other.ModelSpec);
      }
      if (other.input_ != null) {
        if (input_ == null) {
          input_ = new global::Tensorflow.Serving.Input();
        }
        Input.MergeFrom(other.Input);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 10: {
            if (modelSpec_ == null) {
              modelSpec_ = new global::Tensorflow.Serving.ModelSpec();
            }
            input.ReadMessage(modelSpec_);
            break;
          }
          case 18: {
            if (input_ == null) {
              input_ = new global::Tensorflow.Serving.Input();
            }
            input.ReadMessage(input_);
            break;
          }
        }
      }
    }

  }

  public sealed partial class RegressionResponse : pb::IMessage<RegressionResponse> {
    private static readonly pb::MessageParser<RegressionResponse> _parser = new pb::MessageParser<RegressionResponse>(() => new RegressionResponse());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<RegressionResponse> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Tensorflow.Serving.RegressionReflection.Descriptor.MessageTypes[3]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public RegressionResponse() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public RegressionResponse(RegressionResponse other) : this() {
      ModelSpec = other.modelSpec_ != null ? other.ModelSpec.Clone() : null;
      Result = other.result_ != null ? other.Result.Clone() : null;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public RegressionResponse Clone() {
      return new RegressionResponse(this);
    }

    /// <summary>Field number for the "model_spec" field.</summary>
    public const int ModelSpecFieldNumber = 2;
    private global::Tensorflow.Serving.ModelSpec modelSpec_;
    /// <summary>
    /// Effective Model Specification used for regression.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Tensorflow.Serving.ModelSpec ModelSpec {
      get { return modelSpec_; }
      set {
        modelSpec_ = value;
      }
    }

    /// <summary>Field number for the "result" field.</summary>
    public const int ResultFieldNumber = 1;
    private global::Tensorflow.Serving.RegressionResult result_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Tensorflow.Serving.RegressionResult Result {
      get { return result_; }
      set {
        result_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as RegressionResponse);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(RegressionResponse other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(ModelSpec, other.ModelSpec)) return false;
      if (!object.Equals(Result, other.Result)) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (modelSpec_ != null) hash ^= ModelSpec.GetHashCode();
      if (result_ != null) hash ^= Result.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (result_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Result);
      }
      if (modelSpec_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(ModelSpec);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (modelSpec_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(ModelSpec);
      }
      if (result_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Result);
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(RegressionResponse other) {
      if (other == null) {
        return;
      }
      if (other.modelSpec_ != null) {
        if (modelSpec_ == null) {
          modelSpec_ = new global::Tensorflow.Serving.ModelSpec();
        }
        ModelSpec.MergeFrom(other.ModelSpec);
      }
      if (other.result_ != null) {
        if (result_ == null) {
          result_ = new global::Tensorflow.Serving.RegressionResult();
        }
        Result.MergeFrom(other.Result);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 10: {
            if (result_ == null) {
              result_ = new global::Tensorflow.Serving.RegressionResult();
            }
            input.ReadMessage(result_);
            break;
          }
          case 18: {
            if (modelSpec_ == null) {
              modelSpec_ = new global::Tensorflow.Serving.ModelSpec();
            }
            input.ReadMessage(modelSpec_);
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code