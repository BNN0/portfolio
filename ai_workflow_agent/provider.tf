terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }


  backend "http" {
    address        = "https://git.punto-lab.com/api/v4/projects/${CI_PROJECT_ID}/terraform/state/tf_state"
    lock_address   = "https://git.punto-lab.com/api/v4/projects/${CI_PROJECT_ID}/terraform/state/tf_state/lock"
    unlock_address = "https://git.punto-lab.com/api/v4/projects/${CI_PROJECT_ID}/terraform/state/tf_state/unlock"
    username       = "${git_user}"
    password       = "${git_token}"
  }
}