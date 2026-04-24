variable "aws_region" {
    description = "AWS region to launch servers."
    default     = "us-west-2"
}
variable "public_key_path" {
  default = "trantor_key.pub"
}

variable "private_key_path" {
  default = "trantor_key.pem"
}

variable "key_name" {
  default     = "trantor_key"
  description = "Desired name of AWS key pair"
}

variable "aws_amis" {
  default = {
    us-west-2 = "ami-04dd23e62ed049936"
  }
}
