// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: ohtmessage.proto
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace com.mirle.ibg3k0.sc.ProtocolFormat {

  /// <summary>Holder for reflection information generated from ohtmessage.proto</summary>
  public static partial class OhtmessageReflection {

    #region Descriptor
    /// <summary>File descriptor for ohtmessage.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static OhtmessageReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChBvaHRtZXNzYWdlLnByb3RvEgh0dXRvcmlhbCJvCgxBRERSRVNTX0lORk8S",
            "MAoIQUREUkVTU1MYASADKAsyHi50dXRvcmlhbC5BRERSRVNTX0lORk8uQURE",
            "UkVTUxotCgdBRERSRVNTEhIKCkFERFJFU1NfSUQYASABKAkSDgoGQ1JSX0lE",
            "GAIgASgJQoYBChRjb20uZXhhbXBsZS50dXRvcmlhbEIRQWRkcmVzc0Jvb2tQ",
            "cm90b3NQAVorZ2l0aHViLmNvbS9nb2xhbmcvcHJvdG9idWYvcHR5cGVzL3Rp",
            "bWVzdGFtcPgBAaICA0dQQqoCImNvbS5taXJsZS5pYmczazAuc2MuUHJvdG9j",
            "b2xGb3JtYXRiBnByb3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::com.mirle.ibg3k0.sc.ProtocolFormat.ADDRESS_INFO), global::com.mirle.ibg3k0.sc.ProtocolFormat.ADDRESS_INFO.Parser, new[]{ "ADDRESSS" }, null, null, new pbr::GeneratedClrTypeInfo[] { new pbr::GeneratedClrTypeInfo(typeof(global::com.mirle.ibg3k0.sc.ProtocolFormat.ADDRESS_INFO.Types.ADDRESS), global::com.mirle.ibg3k0.sc.ProtocolFormat.ADDRESS_INFO.Types.ADDRESS.Parser, new[]{ "ADDRESSID", "CRRID" }, null, null, null)})
          }));
    }
    #endregion

  }
  #region Messages
  /// <summary>
  ///  [START messages]
  /// </summary>
  public sealed partial class ADDRESS_INFO : pb::IMessage<ADDRESS_INFO> {
    private static readonly pb::MessageParser<ADDRESS_INFO> _parser = new pb::MessageParser<ADDRESS_INFO>(() => new ADDRESS_INFO());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<ADDRESS_INFO> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::com.mirle.ibg3k0.sc.ProtocolFormat.OhtmessageReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ADDRESS_INFO() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ADDRESS_INFO(ADDRESS_INFO other) : this() {
      aDDRESSS_ = other.aDDRESSS_.Clone();
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ADDRESS_INFO Clone() {
      return new ADDRESS_INFO(this);
    }

    /// <summary>Field number for the "ADDRESSS" field.</summary>
    public const int ADDRESSSFieldNumber = 1;
    private static readonly pb::FieldCodec<global::com.mirle.ibg3k0.sc.ProtocolFormat.ADDRESS_INFO.Types.ADDRESS> _repeated_aDDRESSS_codec
        = pb::FieldCodec.ForMessage(10, global::com.mirle.ibg3k0.sc.ProtocolFormat.ADDRESS_INFO.Types.ADDRESS.Parser);
    private readonly pbc::RepeatedField<global::com.mirle.ibg3k0.sc.ProtocolFormat.ADDRESS_INFO.Types.ADDRESS> aDDRESSS_ = new pbc::RepeatedField<global::com.mirle.ibg3k0.sc.ProtocolFormat.ADDRESS_INFO.Types.ADDRESS>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::com.mirle.ibg3k0.sc.ProtocolFormat.ADDRESS_INFO.Types.ADDRESS> ADDRESSS {
      get { return aDDRESSS_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as ADDRESS_INFO);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(ADDRESS_INFO other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if(!aDDRESSS_.Equals(other.aDDRESSS_)) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      hash ^= aDDRESSS_.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      aDDRESSS_.WriteTo(output, _repeated_aDDRESSS_codec);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      size += aDDRESSS_.CalculateSize(_repeated_aDDRESSS_codec);
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(ADDRESS_INFO other) {
      if (other == null) {
        return;
      }
      aDDRESSS_.Add(other.aDDRESSS_);
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
            aDDRESSS_.AddEntriesFrom(input, _repeated_aDDRESSS_codec);
            break;
          }
        }
      }
    }

    #region Nested types
    /// <summary>Container for nested types declared in the ADDRESS_INFO message type.</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static partial class Types {
      public sealed partial class ADDRESS : pb::IMessage<ADDRESS> {
        private static readonly pb::MessageParser<ADDRESS> _parser = new pb::MessageParser<ADDRESS>(() => new ADDRESS());
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static pb::MessageParser<ADDRESS> Parser { get { return _parser; } }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static pbr::MessageDescriptor Descriptor {
          get { return global::com.mirle.ibg3k0.sc.ProtocolFormat.ADDRESS_INFO.Descriptor.NestedTypes[0]; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        pbr::MessageDescriptor pb::IMessage.Descriptor {
          get { return Descriptor; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public ADDRESS() {
          OnConstruction();
        }

        partial void OnConstruction();

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public ADDRESS(ADDRESS other) : this() {
          aDDRESSID_ = other.aDDRESSID_;
          cRRID_ = other.cRRID_;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public ADDRESS Clone() {
          return new ADDRESS(this);
        }

        /// <summary>Field number for the "ADDRESS_ID" field.</summary>
        public const int ADDRESSIDFieldNumber = 1;
        private string aDDRESSID_ = "";
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public string ADDRESSID {
          get { return aDDRESSID_; }
          set {
            aDDRESSID_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
          }
        }

        /// <summary>Field number for the "CRR_ID" field.</summary>
        public const int CRRIDFieldNumber = 2;
        private string cRRID_ = "";
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public string CRRID {
          get { return cRRID_; }
          set {
            cRRID_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
          }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override bool Equals(object other) {
          return Equals(other as ADDRESS);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool Equals(ADDRESS other) {
          if (ReferenceEquals(other, null)) {
            return false;
          }
          if (ReferenceEquals(other, this)) {
            return true;
          }
          if (ADDRESSID != other.ADDRESSID) return false;
          if (CRRID != other.CRRID) return false;
          return true;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override int GetHashCode() {
          int hash = 1;
          if (ADDRESSID.Length != 0) hash ^= ADDRESSID.GetHashCode();
          if (CRRID.Length != 0) hash ^= CRRID.GetHashCode();
          return hash;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override string ToString() {
          return pb::JsonFormatter.ToDiagnosticString(this);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void WriteTo(pb::CodedOutputStream output) {
          if (ADDRESSID.Length != 0) {
            output.WriteRawTag(10);
            output.WriteString(ADDRESSID);
          }
          if (CRRID.Length != 0) {
            output.WriteRawTag(18);
            output.WriteString(CRRID);
          }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public int CalculateSize() {
          int size = 0;
          if (ADDRESSID.Length != 0) {
            size += 1 + pb::CodedOutputStream.ComputeStringSize(ADDRESSID);
          }
          if (CRRID.Length != 0) {
            size += 1 + pb::CodedOutputStream.ComputeStringSize(CRRID);
          }
          return size;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void MergeFrom(ADDRESS other) {
          if (other == null) {
            return;
          }
          if (other.ADDRESSID.Length != 0) {
            ADDRESSID = other.ADDRESSID;
          }
          if (other.CRRID.Length != 0) {
            CRRID = other.CRRID;
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
                ADDRESSID = input.ReadString();
                break;
              }
              case 18: {
                CRRID = input.ReadString();
                break;
              }
            }
          }
        }

      }

    }
    #endregion

  }

  #endregion

}

#endregion Designer generated code
