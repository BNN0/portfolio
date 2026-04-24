resource "aws_iam_instance_profile" "trantor_profile" {
  name = "trantor_profile"
  role = aws_iam_role.trantor_role.name
}

resource "aws_instance" "trantor" {
  ami           = "ami-04dd23e62ed049936"
  instance_type = "t2.micro"
  key_name      = "trantor_key"
  subnet_id     = "subnet-0c1433b27e46e3c09"
  vpc_security_group_ids = [aws_security_group.trantor_sg.id]

  iam_instance_profile = aws_iam_instance_profile.trantor_profile.name
  
  metadata_options {
    http_endpoint               = "enabled"
    http_tokens                 = "required"
    http_put_response_hop_limit = 2
  }

  tags = {
    Name = "trantor"
  }
}
