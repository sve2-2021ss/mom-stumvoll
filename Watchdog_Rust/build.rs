fn main() {
    prost_build::compile_protos(&["src/system_values.proto"], &["src/"]).unwrap();
}