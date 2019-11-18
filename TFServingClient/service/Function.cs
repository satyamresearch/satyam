// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: tensorflow/core/framework/function.proto
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Tensorflow {

  /// <summary>Holder for reflection information generated from tensorflow/core/framework/function.proto</summary>
  public static partial class FunctionReflection {

    #region Descriptor
    /// <summary>File descriptor for tensorflow/core/framework/function.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static FunctionReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Cih0ZW5zb3JmbG93L2NvcmUvZnJhbWV3b3JrL2Z1bmN0aW9uLnByb3RvEgp0",
            "ZW5zb3JmbG93Gip0ZW5zb3JmbG93L2NvcmUvZnJhbWV3b3JrL2F0dHJfdmFs",
            "dWUucHJvdG8aKHRlbnNvcmZsb3cvY29yZS9mcmFtZXdvcmsvbm9kZV9kZWYu",
            "cHJvdG8aJnRlbnNvcmZsb3cvY29yZS9mcmFtZXdvcmsvb3BfZGVmLnByb3Rv",
            "ImoKEkZ1bmN0aW9uRGVmTGlicmFyeRIpCghmdW5jdGlvbhgBIAMoCzIXLnRl",
            "bnNvcmZsb3cuRnVuY3Rpb25EZWYSKQoIZ3JhZGllbnQYAiADKAsyFy50ZW5z",
            "b3JmbG93LkdyYWRpZW50RGVmIrACCgtGdW5jdGlvbkRlZhIkCglzaWduYXR1",
            "cmUYASABKAsyES50ZW5zb3JmbG93Lk9wRGVmEi8KBGF0dHIYBSADKAsyIS50",
            "ZW5zb3JmbG93LkZ1bmN0aW9uRGVmLkF0dHJFbnRyeRIlCghub2RlX2RlZhgD",
            "IAMoCzITLnRlbnNvcmZsb3cuTm9kZURlZhItCgNyZXQYBCADKAsyIC50ZW5z",
            "b3JmbG93LkZ1bmN0aW9uRGVmLlJldEVudHJ5GkIKCUF0dHJFbnRyeRILCgNr",
            "ZXkYASABKAkSJAoFdmFsdWUYAiABKAsyFS50ZW5zb3JmbG93LkF0dHJWYWx1",
            "ZToCOAEaKgoIUmV0RW50cnkSCwoDa2V5GAEgASgJEg0KBXZhbHVlGAIgASgJ",
            "OgI4AUoECAIQAyI7CgtHcmFkaWVudERlZhIVCg1mdW5jdGlvbl9uYW1lGAEg",
            "ASgJEhUKDWdyYWRpZW50X2Z1bmMYAiABKAlCLwoYb3JnLnRlbnNvcmZsb3cu",
            "ZnJhbWV3b3JrQg5GdW5jdGlvblByb3Rvc1AB+AEBYgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Tensorflow.AttrValueReflection.Descriptor, global::Tensorflow.NodeDefReflection.Descriptor, global::Tensorflow.OpDefReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Tensorflow.FunctionDefLibrary), global::Tensorflow.FunctionDefLibrary.Parser, new[]{ "Function", "Gradient" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Tensorflow.FunctionDef), global::Tensorflow.FunctionDef.Parser, new[]{ "Signature", "Attr", "NodeDef", "Ret" }, null, null, new pbr::GeneratedClrTypeInfo[] { null, null, }),
            new pbr::GeneratedClrTypeInfo(typeof(global::Tensorflow.GradientDef), global::Tensorflow.GradientDef.Parser, new[]{ "FunctionName", "GradientFunc" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  /// <summary>
  /// A library is a set of named functions.
  /// </summary>
  public sealed partial class FunctionDefLibrary : pb::IMessage<FunctionDefLibrary> {
    private static readonly pb::MessageParser<FunctionDefLibrary> _parser = new pb::MessageParser<FunctionDefLibrary>(() => new FunctionDefLibrary());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<FunctionDefLibrary> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Tensorflow.FunctionReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public FunctionDefLibrary() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public FunctionDefLibrary(FunctionDefLibrary other) : this() {
      function_ = other.function_.Clone();
      gradient_ = other.gradient_.Clone();
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public FunctionDefLibrary Clone() {
      return new FunctionDefLibrary(this);
    }

    /// <summary>Field number for the "function" field.</summary>
    public const int FunctionFieldNumber = 1;
    private static readonly pb::FieldCodec<global::Tensorflow.FunctionDef> _repeated_function_codec
        = pb::FieldCodec.ForMessage(10, global::Tensorflow.FunctionDef.Parser);
    private readonly pbc::RepeatedField<global::Tensorflow.FunctionDef> function_ = new pbc::RepeatedField<global::Tensorflow.FunctionDef>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::Tensorflow.FunctionDef> Function {
      get { return function_; }
    }

    /// <summary>Field number for the "gradient" field.</summary>
    public const int GradientFieldNumber = 2;
    private static readonly pb::FieldCodec<global::Tensorflow.GradientDef> _repeated_gradient_codec
        = pb::FieldCodec.ForMessage(18, global::Tensorflow.GradientDef.Parser);
    private readonly pbc::RepeatedField<global::Tensorflow.GradientDef> gradient_ = new pbc::RepeatedField<global::Tensorflow.GradientDef>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::Tensorflow.GradientDef> Gradient {
      get { return gradient_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as FunctionDefLibrary);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(FunctionDefLibrary other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if(!function_.Equals(other.function_)) return false;
      if(!gradient_.Equals(other.gradient_)) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      hash ^= function_.GetHashCode();
      hash ^= gradient_.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      function_.WriteTo(output, _repeated_function_codec);
      gradient_.WriteTo(output, _repeated_gradient_codec);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      size += function_.CalculateSize(_repeated_function_codec);
      size += gradient_.CalculateSize(_repeated_gradient_codec);
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(FunctionDefLibrary other) {
      if (other == null) {
        return;
      }
      function_.Add(other.function_);
      gradient_.Add(other.gradient_);
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
            function_.AddEntriesFrom(input, _repeated_function_codec);
            break;
          }
          case 18: {
            gradient_.AddEntriesFrom(input, _repeated_gradient_codec);
            break;
          }
        }
      }
    }

  }

  /// <summary>
  /// A function can be instantiated when the runtime can bind every attr
  /// with a value. When a GraphDef has a call to a function, it must
  /// have binding for every attr defined in the signature.
  ///
  /// TODO(zhifengc):
  ///   * device spec, etc.
  /// </summary>
  public sealed partial class FunctionDef : pb::IMessage<FunctionDef> {
    private static readonly pb::MessageParser<FunctionDef> _parser = new pb::MessageParser<FunctionDef>(() => new FunctionDef());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<FunctionDef> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Tensorflow.FunctionReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public FunctionDef() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public FunctionDef(FunctionDef other) : this() {
      Signature = other.signature_ != null ? other.Signature.Clone() : null;
      attr_ = other.attr_.Clone();
      nodeDef_ = other.nodeDef_.Clone();
      ret_ = other.ret_.Clone();
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public FunctionDef Clone() {
      return new FunctionDef(this);
    }

    /// <summary>Field number for the "signature" field.</summary>
    public const int SignatureFieldNumber = 1;
    private global::Tensorflow.OpDef signature_;
    /// <summary>
    /// The definition of the function's name, arguments, return values,
    /// attrs etc.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Tensorflow.OpDef Signature {
      get { return signature_; }
      set {
        signature_ = value;
      }
    }

    /// <summary>Field number for the "attr" field.</summary>
    public const int AttrFieldNumber = 5;
    private static readonly pbc::MapField<string, global::Tensorflow.AttrValue>.Codec _map_attr_codec
        = new pbc::MapField<string, global::Tensorflow.AttrValue>.Codec(pb::FieldCodec.ForString(10), pb::FieldCodec.ForMessage(18, global::Tensorflow.AttrValue.Parser), 42);
    private readonly pbc::MapField<string, global::Tensorflow.AttrValue> attr_ = new pbc::MapField<string, global::Tensorflow.AttrValue>();
    /// <summary>
    /// Attributes specific to this function definition.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::MapField<string, global::Tensorflow.AttrValue> Attr {
      get { return attr_; }
    }

    /// <summary>Field number for the "node_def" field.</summary>
    public const int NodeDefFieldNumber = 3;
    private static readonly pb::FieldCodec<global::Tensorflow.NodeDef> _repeated_nodeDef_codec
        = pb::FieldCodec.ForMessage(26, global::Tensorflow.NodeDef.Parser);
    private readonly pbc::RepeatedField<global::Tensorflow.NodeDef> nodeDef_ = new pbc::RepeatedField<global::Tensorflow.NodeDef>();
    /// <summary>
    /// By convention, "op" in node_def is resolved by consulting with a
    /// user-defined library first. If not resolved, "func" is assumed to
    /// be a builtin op.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::Tensorflow.NodeDef> NodeDef {
      get { return nodeDef_; }
    }

    /// <summary>Field number for the "ret" field.</summary>
    public const int RetFieldNumber = 4;
    private static readonly pbc::MapField<string, string>.Codec _map_ret_codec
        = new pbc::MapField<string, string>.Codec(pb::FieldCodec.ForString(10), pb::FieldCodec.ForString(18), 34);
    private readonly pbc::MapField<string, string> ret_ = new pbc::MapField<string, string>();
    /// <summary>
    /// A mapping from the output arg names from `signature` to the
    /// outputs from `node_def` that should be returned by the function.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::MapField<string, string> Ret {
      get { return ret_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as FunctionDef);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(FunctionDef other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Signature, other.Signature)) return false;
      if (!Attr.Equals(other.Attr)) return false;
      if(!nodeDef_.Equals(other.nodeDef_)) return false;
      if (!Ret.Equals(other.Ret)) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (signature_ != null) hash ^= Signature.GetHashCode();
      hash ^= Attr.GetHashCode();
      hash ^= nodeDef_.GetHashCode();
      hash ^= Ret.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (signature_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Signature);
      }
      nodeDef_.WriteTo(output, _repeated_nodeDef_codec);
      ret_.WriteTo(output, _map_ret_codec);
      attr_.WriteTo(output, _map_attr_codec);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (signature_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Signature);
      }
      size += attr_.CalculateSize(_map_attr_codec);
      size += nodeDef_.CalculateSize(_repeated_nodeDef_codec);
      size += ret_.CalculateSize(_map_ret_codec);
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(FunctionDef other) {
      if (other == null) {
        return;
      }
      if (other.signature_ != null) {
        if (signature_ == null) {
          signature_ = new global::Tensorflow.OpDef();
        }
        Signature.MergeFrom(other.Signature);
      }
      attr_.Add(other.attr_);
      nodeDef_.Add(other.nodeDef_);
      ret_.Add(other.ret_);
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
            if (signature_ == null) {
              signature_ = new global::Tensorflow.OpDef();
            }
            input.ReadMessage(signature_);
            break;
          }
          case 26: {
            nodeDef_.AddEntriesFrom(input, _repeated_nodeDef_codec);
            break;
          }
          case 34: {
            ret_.AddEntriesFrom(input, _map_ret_codec);
            break;
          }
          case 42: {
            attr_.AddEntriesFrom(input, _map_attr_codec);
            break;
          }
        }
      }
    }

  }

  /// <summary>
  /// GradientDef defines the gradient function of a function defined in
  /// a function library.
  ///
  /// A gradient function g (specified by gradient_func) for a function f
  /// (specified by function_name) must follow the following:
  ///
  /// The function 'f' must be a numerical function which takes N inputs
  /// and produces M outputs. Its gradient function 'g', which is a
  /// function taking N + M inputs and produces N outputs.
  ///
  /// I.e. if we have
  ///    (y1, y2, ..., y_M) = f(x1, x2, ..., x_N),
  /// then, g is
  ///    (dL/dx1, dL/dx2, ..., dL/dx_N) = g(x1, x2, ..., x_N,
  ///                                      dL/dy1, dL/dy2, ..., dL/dy_M),
  /// where L is a scalar-value function of (x1, x2, ..., xN) (e.g., the
  /// loss function). dL/dx_i is the partial derivative of L with respect
  /// to x_i.
  /// </summary>
  public sealed partial class GradientDef : pb::IMessage<GradientDef> {
    private static readonly pb::MessageParser<GradientDef> _parser = new pb::MessageParser<GradientDef>(() => new GradientDef());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<GradientDef> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Tensorflow.FunctionReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public GradientDef() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public GradientDef(GradientDef other) : this() {
      functionName_ = other.functionName_;
      gradientFunc_ = other.gradientFunc_;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public GradientDef Clone() {
      return new GradientDef(this);
    }

    /// <summary>Field number for the "function_name" field.</summary>
    public const int FunctionNameFieldNumber = 1;
    private string functionName_ = "";
    /// <summary>
    /// The function name.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string FunctionName {
      get { return functionName_; }
      set {
        functionName_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "gradient_func" field.</summary>
    public const int GradientFuncFieldNumber = 2;
    private string gradientFunc_ = "";
    /// <summary>
    /// The gradient function's name.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string GradientFunc {
      get { return gradientFunc_; }
      set {
        gradientFunc_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as GradientDef);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(GradientDef other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (FunctionName != other.FunctionName) return false;
      if (GradientFunc != other.GradientFunc) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (FunctionName.Length != 0) hash ^= FunctionName.GetHashCode();
      if (GradientFunc.Length != 0) hash ^= GradientFunc.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (FunctionName.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(FunctionName);
      }
      if (GradientFunc.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(GradientFunc);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (FunctionName.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(FunctionName);
      }
      if (GradientFunc.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(GradientFunc);
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(GradientDef other) {
      if (other == null) {
        return;
      }
      if (other.FunctionName.Length != 0) {
        FunctionName = other.FunctionName;
      }
      if (other.GradientFunc.Length != 0) {
        GradientFunc = other.GradientFunc;
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
            FunctionName = input.ReadString();
            break;
          }
          case 18: {
            GradientFunc = input.ReadString();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
