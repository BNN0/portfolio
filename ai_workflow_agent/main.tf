provider "aws" {
  region = "us-west-2"
}

resource "aws_eip" "trantor_ip" {
  instance = aws_instance.trantor.id
  domain   = "vpc"
}

output "ec2_instance_id" {
  value = aws_instance.trantor.id
}

