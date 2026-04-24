resource "aws_iam_policy" "trantor_policy" {
  name        = "trantor_policy"
  description = "Política para acceso a Bedrock y SSM"
  policy      = jsonencode({
    Version   = "2012-10-17",
    Statement = [
      {
        Effect = "Allow",
        Action = [
          "bedrock:ListModelCustomizationJobs",
          "bedrock:InvokeAgent",
          "bedrock:InvokeModel",
          "bedrock:InvokeBuilder",
          "bedrock:ListFoundationModels",
          "bedrock:GetModelCustomizationJob",
          "ssm:DescribeAssociation",
          "ssm:GetDeployablePatchSnapshotForInstance",
          "ssm:GetDocument",
          "ssm:DescribeDocument",
          "ssm:GetManifest",
          "ssm:GetParameters",
          "ssm:ListAssociations",
          "ssm:PutInventory",
          "ssm:UpdateInstanceAssociationStatus",
          "ssm:UpdateInstanceInformation",
          "ec2messages:AcknowledgeMessage",
          "ec2messages:DeleteMessage",
          "ec2messages:GetEndpoint",
          "ec2messages:GetMessages",
          "ec2messages:SendReply",
          "ssmmessages:CreateControlChannel",
          "ssmmessages:CreateDataChannel",
          "ssmmessages:OpenControlChannel",
          "ssmmessages:OpenDataChannel"
        ],
        Resource = "*"
      }
    ]
  })
}

resource "aws_iam_role" "trantor_role" {
  name               = "trantor_role"
  assume_role_policy = jsonencode({
    Version   = "2012-10-17",
    Statement = [
      {
        Action    = "sts:AssumeRole",
        Effect    = "Allow",
        Principal = {
          Service = [
            "bedrock.amazonaws.com",
            "ec2.amazonaws.com"
          ]
        }
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "trantor_role_policy_attachment" {
  role       = aws_iam_role.trantor_role.name
  policy_arn = aws_iam_policy.trantor_policy.arn
}

resource "aws_iam_user" "trantor_user" {
  name = "trantor_user"
}

resource "aws_iam_user_policy_attachment" "trantor_user_policy_attachment" {
  user       = aws_iam_user.trantor_user.name
  policy_arn = aws_iam_policy.trantor_policy.arn
}